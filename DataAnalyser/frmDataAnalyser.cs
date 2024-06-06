using DataAnalyser.Models;
using DevExpress.Data.Filtering;
using DevExpress.Spreadsheet;
using DevExpress.Xpo;
using System.Text;

using System.Security.Cryptography;
using DevExpress.Xpo.DB;
using DevExpress.XtraRichEdit.Import.Doc;

namespace DataAnalyser
{
    public partial class frmDataAnalyser : Form
    {
        public UnitOfWork WorkUnit { get; set; }
        public XPCollection<Models.Region> Regions { get; set; }

        Models.Region FocusedRegion { get; set; }
        University focusedUniversity { get; set; }
        public frmDataAnalyser()
        {
            string conn = SQLiteConnectionProvider.GetConnectionString(@"data.db");
            //SQLitePCL.raw.SetProvider();
            SQLitePCL.Batteries.Init();
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(conn, AutoCreateOption.DatabaseAndSchema);
            WorkUnit = new UnitOfWork();
            InitializeComponent();
            Regions = new XPCollection<Models.Region>(WorkUnit);
            gridControl1.DataSource = Regions;
            int count = xpCollection1.Count;
        }

        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                string FileName = dlg.FileName;
                Workbook book = new Workbook();
                book.LoadDocument(FileName);
                foreach (var sheet in book.Worksheets)
                {
                    List<Category> columns = new List<Category>();
                    bool blank = false;
                    int row = 1;
                    int i = 0;
                    string refer = $"{chars[i]}{row}";
                    string column = sheet[refer].Value.TextValue;
                    while (!string.IsNullOrEmpty(column))
                    {

                        if (i >= 2)
                        {
                            Category cat = WorkUnit.FindObject<Category>(CriteriaOperator.Parse("Name=?", column));
                            if (cat == null)
                            {
                                cat = new Category(WorkUnit) { Name = column };
                                WorkUnit.CommitChanges();
                            }
                            columns.Add(cat);
                        }
                        else
                        {
                            columns.Add(null);
                        }
                        column = sheet[$"{chars[++i]}{row}"].Value.TextValue;
                    }

                    i = 1;
                    row++;
                    column = sheet[$"{chars[i]}{row}"].Value.TextValue;

                    while (!string.IsNullOrEmpty(column))
                    {
                        string email = column;
                        string hash = getHashSha256(email);//this should get hashed
                                                           //find entrant using hash
                                                           //create entrant if not found

                        University varsity = gridView2.GetFocusedRow() as University;
                        Entrant entrant = WorkUnit.FindObject<Entrant>(CriteriaOperator.Parse($"{nameof(entrant.Hash)}=? and EntrantUniversity.Name=?", hash, varsity.Name));
                        if (entrant == null)
                        {
                            DateTime datestring = sheet[$"A{row}"].Value.DateTimeValue;

                            entrant = new Entrant(WorkUnit)
                            {
                                EntrantUniversity = varsity,
                                Hash = hash,
                                EntryDate = datestring
                            };
                            /*if (datestring != null)
                                entrant.EntryDate = DateTime.Parse(datestring);*/
                        }


                        for (int j = 2; j < columns.Count; j++)
                        {
                            column = sheet[$"{chars[j]}{row}"].Value.TextValue;
                            if (string.IsNullOrWhiteSpace(column))
                                continue;
                            column = column.ReplaceLineEndings(",");
                            column = TrimWhiteSpace(column);                            
                            string[] values = column.Split(',');
                            int[] scores = new int[] { 6, 5, 4, 3, 3, 3, 3, 2, 2, 1 };
                            int count = 0;
                            foreach (string x in values)
                            {
                                string valuestring = x.Trim().ToLower();
                                if (string.IsNullOrWhiteSpace(valuestring))
                                {
                                    continue;
                                }
                                Value newValue = new Value(WorkUnit)
                                {
                                    Category = columns[j],
                                    Entrant = entrant,
                                    StringValue = valuestring,
                                    Score = scores[count>= scores.Length-1? scores.Length - 1 : count++]

                                };
                            }

                        }
                        column = sheet[$"{chars[1]}{++row}"].Value.TextValue;
                    }
                    WorkUnit.CommitChanges();
                }
            }
            xpCollection1.Reload();
        }
        public static string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
           
        }

        private void gridView2_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {

        }

        private void gridView2_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            WorkUnit.CommitChanges();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            //write to excel file somehow
            SaveFileDialog sfd = new SaveFileDialog();
            var result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                Workbook book = new Workbook();
                XPCollection<Value> values = new XPCollection<Value>(WorkUnit);
                values.Sorting.Add(new SortProperty("Entrant.EntrantUniversity.UniversityRegion.Name", SortingDirection.Ascending));
                values.Sorting.Add(new SortProperty("Entrant.EntrantUniversity.Name", SortingDirection.Ascending));
                XPCollection<Category> categories = new XPCollection<Category>(WorkUnit);
                foreach (Category x in categories)
                {
                    var Sheet = book.Worksheets.Add(x.Name);
                    values.Where(v => v.Category == x);
                    values.Criteria = CriteriaOperator.Parse("Category=?", x.Oid);
                    values.Reload();
                    var list = values.GroupBy(x => new { x.StringValue, x.Entrant.EntrantUniversity }).Select(g => new { g.Key, score = g.Sum(z => z.Score) }).ToList();
                    int cols = 1;
                    int rows = 2;
                    Sheet[$"A{1}"].SetValue("Region");
                    Sheet[$"B{1}"].SetValue("University");
                    Sheet[$"C{1}"].SetValue("Response");
                    Sheet[$"D{1}"].SetValue("Score");
                    foreach (var tmp in list)
                    {
                        Sheet[$"A{rows}"].SetValue(tmp.Key.EntrantUniversity.UniversityRegion.Name);
                        Sheet[$"B{rows}"].SetValue(tmp.Key.EntrantUniversity.Name);
                        Sheet[$"C{rows}"].SetValue(tmp.Key.StringValue);
                        Sheet[$"D{rows}"].SetValue(tmp.score);
                        rows++;
                    }
                }
                book.SaveDocument(sfd.FileName);
            }
        }

        private void gridView2_FocusedRowObjectChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowObjectChangedEventArgs e)
        {
            focusedUniversity = e.Row as University;
            SetGrid2();

        }

        void SetGrid2()
        {
            if (focusedUniversity != null && focusedUniversity.Oid > 0)
            {
                gridControl3.DataSource = focusedUniversity.Entrants;
                simpleButton1.Enabled = true;
                gridControl3.Enabled = true;
            }
            else
            {
                gridControl3.DataSource = null;
                simpleButton1.Enabled = false;
                gridControl3.Enabled = false;
            }
        }

        private void gridView1_FocusedRowObjectChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowObjectChangedEventArgs e)
        {
            FocusedRegion = e.Row as Models.Region;
            if (FocusedRegion != null && FocusedRegion.Oid>0)
            {
                gridControl2.DataSource = FocusedRegion.Universities;
                gridControl2.Enabled = true;

            }
            else
            {
                gridControl2.DataSource = null;
                gridControl2.Enabled=false;
                focusedUniversity = null;
                SetGrid2();
            }
        }

        string TrimWhiteSpace(string Value)
        {
            StringBuilder sbOut = new StringBuilder();
            if (!string.IsNullOrEmpty(Value))
            {
                bool IsWhiteSpace = false;
                bool comma = false;
                for (int i = 0; i < Value.Length; i++)
                {
                    if (char.IsWhiteSpace(Value[i])) //Comparion with WhiteSpace
                    {
                        if (!IsWhiteSpace) //Comparison with previous Char
                        {
                            sbOut.Append(Value[i]);
                            IsWhiteSpace = true;
                            comma = false;
                        }
                        else
                        {
                            sbOut.Append(",");
                            comma = true;
                        }
                    }
                    else
                    {
                        IsWhiteSpace = false;
                        sbOut.Append(Value[i]);
                        comma = false;
                    }
                }
            }
            return sbOut.ToString();
        }
    }
}

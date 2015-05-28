using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Detail : Form
    {
        public Detail(int[,] HuffDec)
        {
            InitializeComponent();

            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;

            //Add column header
            listView1.Columns.Add("Symbol", 100);
            listView1.Columns.Add("Length", 100);

            //Add items in the listview
            string[] arr = new string[4];
            ListViewItem item;

            for (int i = 0; i < 1024; i++) 
            {
                arr[0] = "" + HuffDec[i,0];
                arr[1] = "" + HuffDec[i,1];
                item = new ListViewItem(arr);
                listView1.Items.Add(item);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}

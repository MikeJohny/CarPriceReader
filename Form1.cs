using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CarPriceReader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;

        }
         private void MainForm_Load(object sender, EventArgs e)
        {
            button1.Location = new Point(
                (this.ClientSize.Width - button1.Width) / 2,
                (this.ClientSize.Height - button1.Height) / 2
            );
            dataGridView1.Visible = false;
            // Configure DataGridView properties
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AllowUserToAddRows = false; 
            dataGridView1.RowHeadersVisible = false; 
            dataGridView1.ReadOnly = true;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
           
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle
            {
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Navy,
            };
            dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
        }

        private DataTable LoadDataFromXml(string filePath) {
            // Create dictionaries to store the totals
            Dictionary<string, decimal> modelTotals = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dphTotals = new Dictionary<string, decimal>();

            try
            {
                // Load XML into a DataSet
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(filePath);

                if (dataSet.Tables.Count > 0)
                {
                    DataTable originalTable = dataSet.Tables[0];

                    foreach (DataRow row in originalTable.Rows)
                    {
                        if (DateTime.TryParse(row["SaleDate"].ToString(), out DateTime saleDate))
                        {
                            // Check if date is weekend
                            if (saleDate.DayOfWeek == DayOfWeek.Saturday || saleDate.DayOfWeek == DayOfWeek.Sunday)
                            {
                                string model = row["Model"].ToString();
                                decimal price = Convert.ToDecimal(row["Price"]);
                                decimal dph = Convert.ToDecimal(row["DPH"]);

                                if (modelTotals.ContainsKey(model))
                                {
                                    modelTotals[model] += price;
                                    dphTotals[model] += price * (1 + dph / 100);
                                }
                                else
                                {
                                    modelTotals[model] = price;
                                    dphTotals[model] = price * (1 + dph / 100);
                                }
                            }
                        }
                    }

                    // Create and return the table
                    DataTable summaryTable = new DataTable();
                    summaryTable.Columns.Add("ModelName\nTotalPrice", typeof(string));
                    summaryTable.Columns.Add("TotalPriceDPH", typeof(decimal)); 

                    foreach (var item in modelTotals)
                    {
                        string modelWithPrice = $"{item.Key}\n{item.Value:N2}";
                        decimal priceWithDPH = dphTotals[item.Key];
                        summaryTable.Rows.Add(modelWithPrice, priceWithDPH);
                        
                    }

                    return summaryTable;
                }
                else
                {
                    throw new Exception("The XML file does not contain any tabular data.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading XML: {ex.Message}");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Open XML File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    DataTable summaryTable = LoadDataFromXml(filePath);
                    if (summaryTable != null)
                    { 
                        // Bind the table to the DataGridView
                        dataGridView1.DataSource = summaryTable;
                        dataGridView1.Columns[1].DefaultCellStyle.Format = "N2";
                        dataGridView1.Columns[0].HeaderText = "Název modelu\nCena bez DPH";
                        dataGridView1.Columns[1].HeaderText = "Cena s DPH";
                        dataGridView1.Visible = true;

                        
                        //Resize window to fit the table and move button
                        int totalWidth = dataGridView1.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + dataGridView1.RowHeadersWidth;
                        int totalHeight = dataGridView1.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + dataGridView1.ColumnHeadersHeight;
                        dataGridView1.Width = totalWidth + 2; 
                        dataGridView1.Height = totalHeight + 2;

                        this.ClientSize = new Size(dataGridView1.Location.X + dataGridView1.Width + 20,
                                                   dataGridView1.Location.Y + dataGridView1.Height + button1.Height + 30);
                        button1.Location = new Point(
                            dataGridView1.Location.X + dataGridView1.Width - button1.Width,
                            dataGridView1.Location.Y + dataGridView1.Height + 10 
                        );
                    }
                       
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


    }
}

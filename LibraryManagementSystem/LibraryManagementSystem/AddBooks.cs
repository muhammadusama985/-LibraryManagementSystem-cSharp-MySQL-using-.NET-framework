using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace LibraryManagementSystem
{
    public partial class AddBooks : UserControl
    {
        public event EventHandler BookAdded;

        SqlConnection connect = new SqlConnection(@"Data Source=DESKTOP-P0MS7LE\SQLEXPRESS;Initial Catalog=LMS;Integrated Security=True");

        public AddBooks()
        {
            InitializeComponent();
            displayBooks();
        }

        public void refreshData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)refreshData);
                return;
            }
            displayBooks();
        }

        private String imagePath;

        private void addBooks_importBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image Files (*.jpg; *.png)|*.jpg;*.png";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    imagePath = dialog.FileName;
                    addBooks_picture.ImageLocation = imagePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void addBooks_addBtn_Click(object sender, EventArgs e)
        {
            if (addBooks_picture.Image == null
                || addBooks_bookTitle.Text == ""
                || addBooks_author.Text == ""
                || string.IsNullOrEmpty(addBooks_published.Text)
                || addBooks_status.Text == "")
            {
                MessageBox.Show("Please fill all blank fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    if (connect.State == ConnectionState.Closed)
                    {
                        connect.Open();
                        string insertData = "INSERT INTO books " +
                            "(books_title, author, published_date, status, image, date_insert) " +
                            "VALUES(@bookTitle, @author, @published_date, @status, @image, @dateInsert)";

                        string path = Path.Combine(@"C:\Users\moham\Source\Repos\Library-Management-System-using-CSharp\LibraryManagementSystem\LibraryManagementSystem\Books_Directory\",
                            addBooks_bookTitle.Text.Trim() + addBooks_author.Text.Trim() + ".jpg");

                        string directoryPath = Path.GetDirectoryName(path);

                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        File.Copy(addBooks_picture.ImageLocation, path, true);

                        using (SqlCommand cmd = new SqlCommand(insertData, connect))
                        {
                            cmd.Parameters.AddWithValue("@bookTitle", addBooks_bookTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@author", addBooks_author.Text.Trim());
                            cmd.Parameters.AddWithValue("@status", addBooks_status.Text.Trim());
                            cmd.Parameters.AddWithValue("@image", path);
                            cmd.Parameters.AddWithValue("@published_date", DateTime.Parse(addBooks_published.Text));
                            cmd.Parameters.AddWithValue("@dateInsert", DateTime.Today);

                            cmd.ExecuteNonQuery();

                            displayBooks();

                            MessageBox.Show("Added successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            clearFields();
                            OnBookAdded(EventArgs.Empty);


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        protected virtual void OnBookAdded(EventArgs e)
        {
            BookAdded?.Invoke(this, e);
        }
        public void clearFields()
        {
            addBooks_bookTitle.Text = "";
            addBooks_author.Text = "";
            addBooks_picture.Image = null;
            addBooks_status.SelectedIndex = -1;
        }

        public void displayBooks()
        {
            try
            {
                if (connect.State != ConnectionState.Open)
                {
                    connect.Open();
                }
                SqlCommand cmd = new SqlCommand("SELECT * FROM books where date_delete is null;", connect);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connect.State == ConnectionState.Open)
                {
                    connect.Close();
                }
            }
        }




        private int bookID = 0;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                bookID = (int)row.Cells[0].Value;
                addBooks_bookTitle.Text = row.Cells[1].Value.ToString();
                addBooks_author.Text = row.Cells[2].Value.ToString();
                addBooks_published.Text = row.Cells[5].Value.ToString();

                string imagePath = row.Cells[8].Value.ToString();

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    addBooks_picture.Image = Image.FromFile(imagePath);
                }
                else
                {
                    addBooks_picture.Image = null;
                }
                addBooks_status.Text = row.Cells[4].Value.ToString();
            }
        }

        private void addBooks_clearBtn_Click(object sender, EventArgs e)
        {
            clearFields();
        }

        private void addBooks_updateBtn_Click(object sender, EventArgs e)
        {
            if (addBooks_picture.Image == null
                || addBooks_bookTitle.Text == ""
                || addBooks_author.Text == ""
                || addBooks_published.Text == ""
                || addBooks_status.Text == "")
            {
                MessageBox.Show("Please fill in all fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    // Assuming `connect` is properly instantiated elsewhere
                    if (connect.State != ConnectionState.Open)
                    {
                        DialogResult check = MessageBox.Show("Are you sure you want to UPDATE Book ID:" + bookID + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (check == DialogResult.Yes)
                        {
                            connect.Open();
                            string updateData = "UPDATE books SET books_title = @bookTitle, author = @author, published_date = @published_date, status = @status, date_update = @dateUpdate WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(updateData, connect))
                            {
                                cmd.Parameters.AddWithValue("@bookTitle", addBooks_bookTitle.Text.Trim());
                                cmd.Parameters.AddWithValue("@author", addBooks_author.Text.Trim());
                                cmd.Parameters.AddWithValue("@published_date", DateTime.Parse(addBooks_published.Text));
                                cmd.Parameters.AddWithValue("@status", addBooks_status.Text.Trim());
                                cmd.Parameters.AddWithValue("@id", bookID);
                                cmd.Parameters.AddWithValue("@dateUpdate", DateTime.Today);

                                cmd.ExecuteNonQuery();

                                displayBooks();

                                MessageBox.Show("Updated successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                clearFields();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cancelled.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }


        private void addBooks_deleteBtn_Click(object sender, EventArgs e)
        {
            if (addBooks_picture.Image == null
                || addBooks_bookTitle.Text == ""
                || addBooks_author.Text == ""
                || addBooks_published.Text == ""
                || addBooks_status.Text == "")
            {
                MessageBox.Show("Please select item first", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    if (connect.State != ConnectionState.Open)
                    {
                        DialogResult check = MessageBox.Show("Are you sure you want to DELETE Book ID:" + bookID + "?", "Confirmation Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (check == DialogResult.Yes)
                        {
                            connect.Open();
                            string deleteData = "DELETE FROM books WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(deleteData, connect))
                            {
                                cmd.Parameters.AddWithValue("@id", bookID);

                                cmd.ExecuteNonQuery();

                                displayBooks();

                                MessageBox.Show("Deleted successfully!", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                clearFields();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Cancelled.", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text.Trim();

            // If the search term is empty, display all books
            if (string.IsNullOrEmpty(searchTerm))
            {
                displayBooks();
                return;
            }

            // Filter the DataGridView based on the search term
            DataView dv = new DataView((DataTable)dataGridView1.DataSource);

            // Filter by books_title and author (assuming they are string columns)
            string filterExpression = $"books_title LIKE '%{searchTerm}%' OR author LIKE '%{searchTerm}%'";

            // Parse the search term as a DateTime to filter by published_date
            DateTime parsedDate;
            if (DateTime.TryParse(searchTerm, out parsedDate))
            {
                filterExpression += $" OR published_date = '{parsedDate.ToShortDateString()}'";
            }

            // Filter by status (assuming it's a string column)
            filterExpression += $" OR status LIKE '%{searchTerm}%'";

            // Filter by ID (assuming it's an integer column)
            int parsedId;
            if (int.TryParse(searchTerm, out parsedId))
            {
                filterExpression += $" OR id = {parsedId}";
            }

            dv.RowFilter = filterExpression;

            // Update the DataGridView with filtered data
            dataGridView1.DataSource = dv.ToTable();
        }
    }
}

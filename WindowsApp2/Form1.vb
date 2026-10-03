Imports MySql.Data.MySqlClient
Public Class Form1

    ' Shared connection string across the form
    Private connStr As String = "Server=localhost;Database=retail_analysis_sn;Uid=root;Pwd=013107;"

    ' ------------------------------------------------------------------
    ' FORM LOAD: Load initial data into DataGridView
    ' ------------------------------------------------------------------
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadData()
        Using conn As New MySqlConnection(connStr)
            Try
                conn.Open()
                MessageBox.Show("Successfully connected to MySQL!")
            Catch ex As Exception
                MessageBox.Show("Connection failed: " & ex.Message)
            End Try
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' HELPER METHOD: Fetch & Display Products
    ' ------------------------------------------------------------------
    Private Sub LoadData()
        Using conn As New MySqlConnection(connStr)
            Try
                Dim query As String = "SELECT * FROM tblProducts"
                Dim adapter As New MySqlDataAdapter(query, conn)
                Dim tblProducts As New DataTable()

                ' Fill DataTable and bind to DataGridView
                adapter.Fill(tblProducts)
                DataGridView1.DataSource = tblProducts

            Catch ex As Exception
                MessageBox.Show("Error loading data: " & ex.Message,
                                "Database Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' SAVE BUTTON: Insert new record into tblProducts
    ' ------------------------------------------------------------------
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        ' 1. Validate numerical inputs
        Dim productID As Integer
        Dim price As Decimal
        Dim quantity As Integer

        If Not Integer.TryParse(txtProductID.Text.Trim(), productID) Then
            MessageBox.Show("Please enter a valid numeric Product ID.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(txtProductName.Text) Then
            MessageBox.Show("Product Name cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If Not Decimal.TryParse(txtPrice.Text.Trim(), price) Then
            MessageBox.Show("Please enter a valid Price.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If Not Integer.TryParse(txtQuantity.Text.Trim(), quantity) Then
            MessageBox.Show("Please enter a valid Quantity.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        ' 2. Prepare parameterized query
        Dim query As String = "INSERT INTO tblProducts (productID, productName, category, price, quantity, supplier) " &
                             "VALUES (@productID, @productName, @category, @price, @quantity, @supplier)"

        ' 3. Execute INSERT query
        Using conn As New MySqlConnection(connStr)
            Using cmd As New MySqlCommand(query, conn)

                cmd.Parameters.AddWithValue("@productID", productID)
                cmd.Parameters.AddWithValue("@productName", txtProductName.Text.Trim())
                cmd.Parameters.AddWithValue("@category", txtCategory.Text.Trim())
                cmd.Parameters.AddWithValue("@price", price)
                cmd.Parameters.AddWithValue("@quantity", quantity)
                cmd.Parameters.AddWithValue("@supplier", txtSupplier.Text.Trim())

                Try
                    conn.Open()
                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        MessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                        ' Clear input fields & reload DataGridView
                        ClearInputs()
                        LoadData()
                    End If

                Catch ex As MySqlException
                    ' Handle duplicate primary key error
                    If ex.Number = 1062 Then
                        MessageBox.Show("A product with this Product ID already exists.", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Else
                        MessageBox.Show("Database Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End Try

            End Using
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' SEARCH BUTTON: Find record by Product ID and populate TextBoxes
    ' ------------------------------------------------------------------
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim productID As Integer

        If Not Integer.TryParse(txtProductID.Text.Trim(), productID) Then
            MessageBox.Show("Please enter a valid numeric Product ID to search.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim query As String = "SELECT * FROM tblProducts WHERE productID = @productID"

        Using conn As New MySqlConnection(connStr)
            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@productID", productID)

                Try
                    conn.Open()
                    Dim reader As MySqlDataReader = cmd.ExecuteReader()

                    If reader.Read() Then
                        ' Record found — populate form controls
                        txtProductName.Text = reader("productName").ToString()
                        txtCategory.Text = reader("category").ToString()
                        txtPrice.Text = reader("price").ToString()
                        txtQuantity.Text = reader("quantity").ToString()
                        txtSupplier.Text = reader("supplier").ToString()

                        MessageBox.Show("Product found!", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        MessageBox.Show("No product found with Product ID: " & productID, "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If

                    reader.Close()

                Catch ex As MySqlException
                    MessageBox.Show("Search Error: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' UPDATE BUTTON: Save changes to an existing product
    ' ------------------------------------------------------------------
    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        ' 1. Input Validation
        Dim productID As Integer
        Dim price As Decimal
        Dim quantity As Integer

        If Not Integer.TryParse(txtProductID.Text.Trim(), productID) Then
            MessageBox.Show("Please enter a valid numeric Product ID.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(txtProductName.Text) Then
            MessageBox.Show("Product Name cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If Not Decimal.TryParse(txtPrice.Text.Trim(), price) Then
            MessageBox.Show("Please enter a valid Price.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If Not Integer.TryParse(txtQuantity.Text.Trim(), quantity) Then
            MessageBox.Show("Please enter a valid Quantity.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        ' 2. SQL UPDATE Query
        Dim query As String = "UPDATE tblProducts SET " &
                             "productName = @productName, " &
                             "category = @category, " &
                             "price = @price, " &
                             "quantity = @quantity, " &
                             "supplier = @supplier " &
                             "WHERE productID = @productID"

        ' 3. Execute Update
        Using conn As New MySqlConnection(connStr)
            Using cmd As New MySqlCommand(query, conn)

                cmd.Parameters.AddWithValue("@productID", productID)
                cmd.Parameters.AddWithValue("@productName", txtProductName.Text.Trim())
                cmd.Parameters.AddWithValue("@category", txtCategory.Text.Trim())
                cmd.Parameters.AddWithValue("@price", price)
                cmd.Parameters.AddWithValue("@quantity", quantity)
                cmd.Parameters.AddWithValue("@supplier", txtSupplier.Text.Trim())

                Try
                    conn.Open()
                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        ClearInputs()
                        LoadData()
                    Else
                        MessageBox.Show("Update failed. Make sure the Product ID exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    End If

                Catch ex As MySqlException
                    MessageBox.Show("Database Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try

            End Using
        End Using
    End Sub

    ' ------------------------------------------------------------------
    ' OPTIONAL: Fill textboxes automatically when clicking a DataGridView row
    ' ------------------------------------------------------------------
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex >= 0 Then
            Dim row As DataGridViewRow = DataGridView1.Rows(e.RowIndex)

            txtProductID.Text = row.Cells("productID").Value.ToString()
            txtProductName.Text = row.Cells("productName").Value.ToString()
            txtCategory.Text = row.Cells("category").Value.ToString()
            txtPrice.Text = row.Cells("price").Value.ToString()
            txtQuantity.Text = row.Cells("quantity").Value.ToString()
            txtSupplier.Text = row.Cells("supplier").Value.ToString()
        End If
    End Sub

    ' ------------------------------------------------------------------
    ' HELPER METHOD: Clear TextBoxes after saving
    ' ------------------------------------------------------------------
    Private Sub ClearInputs()
        txtProductID.Clear()
        txtProductName.Clear()
        txtCategory.Clear()
        txtPrice.Clear()
        txtQuantity.Clear()
        txtSupplier.Clear()
    End Sub

End Class

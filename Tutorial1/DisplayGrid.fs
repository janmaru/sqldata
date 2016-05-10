namespace Mahamudra.System.Drawing
    open System.Drawing
    open System.Windows.Forms

    module δ =
        let [<Literal>] ICO = __SOURCE_DIRECTORY__ + @"\data\favicon-192x192.ico" 
        let draw rows label_text =
            let form = new Form(Visible = true, 
                                WindowState = FormWindowState.Maximized,
                                Icon =new Icon(ICO),
                                Text = label_text,
                                TopMost = true, 
                                Size = Size(600,600))

            let data = new DataGridView(Dock = DockStyle.Fill, 
                                        Text = label_text,
                                        Font = new Font("Lucida Console",12.0f),
                                        ForeColor = Color.DarkBlue)

            form.Controls.Add(data)
            data.DataSource <- rows 
            data.Columns.[0].Width <- 40 //ID

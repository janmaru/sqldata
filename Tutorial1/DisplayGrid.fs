namespace Mahamudra.System.Drawing

open System.Drawing
open System.Windows.Forms

module δ = 
    [<Literal>]
    let ICO = __SOURCE_DIRECTORY__ + @"\data\favicon-192x192.ico"
    
    let draw rows label_text = 
        let form = 
            new Form(Visible = true, WindowState = FormWindowState.Maximized, Icon = new Icon(ICO), Text = label_text, 
                     TopMost = true, Size = Size(600, 600))
        let data = 
            new DataGridView(Dock = DockStyle.Fill, Text = label_text, Font = new Font("Lucida Console", 12.0f), 
                             ForeColor = Color.DarkBlue)

        form.Controls.Add(data)
        data.DataSource <- rows |> List.toArray

        for i in 0 .. data.Columns.Count-1 do
             data.Columns.[i].AutoSizeMode<- DataGridViewAutoSizeColumnMode.Fill

//Α α alpha, άλφα [a] [aː] [a] 
//Β β beta, βήτα [b] [v] 
//Γ γ gamma, γάμμα [ɡ], [ŋ][7] [ɣ] ~ [ʝ],
//[ŋ][8] ~ [ɲ][9] 
//Δ δ delta, δέλτα [d] [ð] 
//Ε ε epsilon, έψιλον [e] [e] 
//Ζ ζ zeta, ζήτα [zd]A [z] 
//Η η eta, ήτα [ɛː] [i] 
//Θ θ theta, θήτα [tʰ] [θ] 
//Ι ι iota, ιώτα [i] [iː] [i], [ʝ],[10] [ɲ][11] 
//Κ κ kappa, κάππα [k] [k] ~ [c] 
//Λ λ lambda, λάμδα [l] [l] 
//Μ μ 
//Ν ν nu, νυ [n] [n] 
//Ξ ξ xi, ξι [ks] [ks] 
//Ο ο omicron, όμικρον [o] [o] 
//Π π pi, πι [p] [p] 
//Ρ ρ rho, ρώ [r] [r] 
//Σ σ/ς[13] sigma, σίγμα [s] [s] 
//Τ τ tau, ταυ [t] [t] 
//Υ υ upsilon, ύψιλον [y] [yː] [i] 
//Φ φ phi, φι [pʰ] [f] 
//Χ χ chi, χι [kʰ] [x] ~ [ç] 
//Ψ ψ psi, ψι [ps] [ps] 
//Ω ω omega, ωμέγα 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MiniERP
{
    public partial class frmERP : Form
    {
        public const string CONNECTIONSTRING = "Driver={Microsoft Access Driver (*.mdb)};DBQ=minierp.mdb";

        private OdbcConnection cn;
        private OdbcDataAdapter da;
        DataSet ds;

        //Construct
        public frmERP()
        {
            cn = new OdbcConnection(CONNECTIONSTRING);
            InitializeComponent();
        }

        //Events
        #region Importacions
        private void articlesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportarArticles();
        }

        private void proveïdorsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportarProveidors();
        }

        private void incorporarComandaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nomFitxer;
            openFileDialog1.InitialDirectory = Application.StartupPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nomFitxer = openFileDialog1.FileName;

                ImportarComanda(nomFitxer);
            }
        }

        private void recepcionarAlbaràToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nomFitxer;
            openFileDialog1.InitialDirectory = Application.StartupPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nomFitxer = openFileDialog1.FileName;

                IncorporaAlbara(nomFitxer);
            }

        }
        #endregion    

        #region Exportacions
        private void articlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nomFitxer;
            saveFileDialog1.FileName = "articles.xml"; 
            saveFileDialog1.InitialDirectory = Application.StartupPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nomFitxer = saveFileDialog1.FileName;
                ExportarArticles(nomFitxer);
            }
        }

        private void proveïdorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nomFitxer;
            saveFileDialog1.FileName = "proveidors.xml"; 
            saveFileDialog1.InitialDirectory = Application.StartupPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nomFitxer = saveFileDialog1.FileName;
                ExportarProveidors(nomFitxer);
            }
        }
        #endregion

        #region LListats
        private void articlesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string nomFitxer;
            saveFileDialog1.FileName = "articles.xml";
            saveFileDialog1.InitialDirectory = Application.StartupPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nomFitxer = saveFileDialog1.FileName;
                ExportarArticles(nomFitxer);

                System.Diagnostics.Process.Start("IExplore.exe", nomFitxer);
            }
        }

        private void proveïdorsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string nomFitxer;
            saveFileDialog1.FileName = "proveidors.xml";
            saveFileDialog1.InitialDirectory = Application.StartupPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nomFitxer = saveFileDialog1.FileName;
                ExportarProveidors(nomFitxer);

                System.Diagnostics.Process.Start("IExplore.exe", nomFitxer);
            }
        }

        #endregion


        //Methods
        #region Importacions
        private void ImportarArticles()
        {
            const string RUTAXML = "articles.xml";

            bool error = false;
            string codi;
            string descripcio;
            int stock;
            int preu;
            XmlDocument xml;
            XmlNodeList xnList;
            OdbcCommand cmd;

            if (ValidarXML(RUTAXML, "articles.xsd"))
            {
                cn.Open(); //Obrir el access

                xml = new XmlDocument();
                xml.Load(RUTAXML);
                xnList = xml.SelectNodes("/articles/article");

                cmd = new OdbcCommand();
                cmd.Connection = cn;

                #region Insertar articles               
                foreach (XmlNode xn in xnList)
                {
                    //Obtenir les dades
                    codi = xn["codi"].InnerText;
                    if (ExisteixArticle(codi)) error = true;
                    else
                    {
                        descripcio = xn["descripcio"].InnerText;
                        stock = Convert.ToInt32(xn["estoc"].InnerText);
                        preu = Convert.ToInt32(xn["preu"].InnerText);

                        //Insert
                        cmd.CommandText = "INSERT INTO article VALUES ('" + codi + "','" + descripcio + "'," + stock + "," + preu + ");";
                        cmd.ExecuteNonQuery(); 
                    }
                                          
                }

                if (error)
                {
                    AfegirError("Importar articles", "Importacio d'articles ja existents");
                    MessageBox.Show("Hi ha articles que ja existeixen", "Error d'inserció", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else MessageBox.Show("Importació realitzada correctament", "Importacio correcta");   
                #endregion
                

                cn.Close(); //Tencar el access
                
            }
            else MessageBox.Show("FITXER XML D'ARTICLES NO VÀLID", "Error de validació", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }     

        private void ImportarProveidors()
        {
            string RUTAXML = "proveidors.xml";

            bool error = false;
            string codi;
            string nom;
            string direccio;
            string poble;
            string cPostal;
            XmlDocument xml;
            XmlNodeList xnList;
            OdbcCommand cmd;

            if (ValidarXML(RUTAXML, "proveidors.xsd"))
            {
                cn.Open(); //Obrir access

                xml = new XmlDocument();
                xml.Load(RUTAXML);
                xnList = xml.SelectNodes("/proveidors/proveidor");

                cmd = new OdbcCommand();
                cmd.Connection = cn;

                #region Insertar Proveidors

                foreach (XmlNode xn in xnList)
                {
                    //obtenim del xml
                    codi = xn["codi"].InnerText;
                    nom = xn["nom"].InnerText;
                    direccio = xn["adreça"].InnerText;
                    poble = xn["poblacio"].InnerText;
                    cPostal = xn["cp"].InnerText;

                    if (ExisteixProveidor(codi)) error = true;
                    else
                    {
                        //Insert
                        cmd.CommandText = "INSERT INTO proveidor VALUES ('" + codi + "','" + nom + "','" + direccio + "','" + poble + "','" + cPostal + "');";
                        cmd.ExecuteNonQuery();
                    }                   
                }

                if (error)
                {
                    AfegirError("Importar proveidors", "Importacio de proveidors ja existent");
                    MessageBox.Show("Hi ha proveidors que  ja existeixen!", "Error d'inserció", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else MessageBox.Show("Importació realitzada correctament", "Importacio correcta");            
                #endregion

                cn.Close(); //Tencar el access
                
            }
            else MessageBox.Show("FITXER XML D'ARTICLES NO VÀLID", "Error de validació", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ImportarComanda(string xmlFilename)
        {
            bool error = false;
            int autonumComanda;
            string codiProv;
            DateTime data;
            string codiArt;
            int quant;
            int preu;            
            XmlDocument xml;
            XmlNode xn;
            XmlNodeList xnListArticles;
            OdbcCommand cmd;

            if (ValidarXML(xmlFilename, "comanda.xsd"))
            {
                cn.Open(); //Obrir el access

                xml = new XmlDocument();
                xml.Load(xmlFilename);
                xn = xml.SelectSingleNode("/comanda");
                cmd = new OdbcCommand();
                cmd.Connection = cn;

                //CCOMANDA  
                codiProv = xn["codiProv"].InnerText;
                if (!ExisteixProveidor(codiProv))
                {
                    AfegirError("Importar comanda", "Importacio comanda amb proveidor inexistent");
                    MessageBox.Show("Proveidor inexistent", "Error d'inserció", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                { 
                    data = Convert.ToDateTime(xn["data"].InnerText);
                    xnListArticles = xn.SelectNodes("artices/article");
                    cmd.CommandText = "INSERT INTO ccomanda(codiproveidor, data) VALUES ('" + codiProv + "', '" + data + "');";
                    cmd.ExecuteNonQuery();

                    //Obtenir el id autonumeric
                    autonumComanda = ObtenirId();

                    //DCOMANDA
                    #region Insertar Articles
                    xnListArticles = xn.SelectNodes("articles/article");
                    foreach (XmlNode xnArt in xnListArticles)
                    {
                        codiArt = xnArt["codi"].InnerText;
                        if (!ExisteixArticle(codiArt)) error = true;
                        else
                        {
                            quant = Convert.ToInt32(xnArt["quant"].InnerText);
                            preu = Convert.ToInt32(xnArt["preu"].InnerText);
                            cmd.CommandText = "INSERT INTO dcomanda VALUES ('" + autonumComanda + "', '" + codiArt + "', " + quant + ", " + preu + ", false);";
                            cmd.ExecuteNonQuery();
                        }

                    }

                    if (error)
                    {
                        AfegirError("Importar comanda", "Comanda amb articles no existents");
                        MessageBox.Show("Un o més articles no existeixen en la comanda", "Error d'incorporació", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else MessageBox.Show("Comanda incorporada correctament", "Incorporació correcta");
                    #endregion
                }
                cn.Close(); //Tencar el access
                
            }
            else MessageBox.Show("FITXER XML D'ARTICLES NO VÀLID", "Error de validació", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void IncorporaAlbara(string xmlFilename)
        {
            bool error = false;
            int autonumAlbara;
            int codiComanda;
            string codiArt;
            int quant;
            int preu;
            DateTime data;
            XmlDocument xml;
            XmlNode xn;
            XmlNodeList xnListArticles;
            OdbcCommand cmd;

            if (ValidarXML(xmlFilename, "albara.xsd"))
            {
                cn.Open(); //Obrir el access

                xml = new XmlDocument();
                xml.Load(xmlFilename);
                xn = xml.SelectSingleNode("/albara");
                cmd = new OdbcCommand();
                cmd.Connection = cn;

                //CALBARA
                codiComanda = Convert.ToInt32(xn["codiComanda"].InnerText);
                if (!ExisteixComanda(codiComanda))
                {
                    AfegirError("Incorporacio d'albará", "Albará d'una comanda inexistent");
                    MessageBox.Show("Aquest albará correspon a una comanda inexistent", "Error d'inserció", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    data = Convert.ToDateTime(xn["data"].InnerText);
                    cmd.CommandText = "INSERT INTO calbara(codicomanda, data) VALUES ('" + codiComanda + "', '" + data + "');";
                    cmd.ExecuteNonQuery();

                    autonumAlbara = ObtenirId();

                    //DALBARA
                    #region Insertar Articles
                    xnListArticles = xn.SelectNodes("articles/article");
                    foreach (XmlNode xnArt in xnListArticles)
                    {
                        codiArt = xnArt["codi"].InnerText;
                        if (!ExisteixArticleEnComanda(codiArt, codiComanda)) error = true;
                        else
                        {
                            quant = Convert.ToInt32(xnArt["quant"].InnerText);
                            preu = Convert.ToInt32(xnArt["preu"].InnerText);
                            cmd.CommandText = "INSERT INTO dalbara VALUES ('" + autonumAlbara + "', '" + codiArt + "', " + quant + ", " + preu + ");";
                            ActualitzarArticleRebut(codiComanda, codiArt);
                            AfegirStock(codiArt, quant);
                            cmd.ExecuteNonQuery();
                        }                      
                    }

                    if (error)
                    {
                        AfegirError("Incorporacio d'albará", "Article no existent en la comanda");
                        MessageBox.Show("Un o més articles no existeixen en la comanda", "Error d'incorporació", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else MessageBox.Show("Incorporació realitzada correctament", "Incorporació correcta");
                    #endregion
                }
                cn.Close(); //Tencar access
            }
        }
        #endregion

        #region Exportacions
        private void ExportarArticles(string filename)
        {
            System.IO.FileStream fs;
            System.IO.StreamWriter sw;

            da = new OdbcDataAdapter("SELECT * FROM article", cn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) MessageBox.Show("No hi ha articles per exportar!", "Sense articles", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
            {
                fs = new FileStream(filename, System.IO.FileMode.OpenOrCreate);
                sw = new StreamWriter(fs);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<articles xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"articles.xsd\">");
               
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    sw.WriteLine("  <article>");
                    sw.WriteLine("      <codi>" + row[0] + "</codi>");
                    sw.WriteLine("      <descripcio>" + row[1] + "</descripcio>");
                    sw.WriteLine("      <estoc>" + row[2] + "</estoc>");
                    sw.WriteLine("      <preu>" + row[3] + "</preu>");
                    sw.WriteLine("  </article>");
                }
                sw.WriteLine("</articles>");
                sw.Close();
                fs.Close();
            }          
        }

        private void ExportarProveidors(string filename)
        {
            System.IO.FileStream fs;
            System.IO.StreamWriter sw;

            da = new OdbcDataAdapter("SELECT * FROM proveidor", cn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) MessageBox.Show("No hi ha proveidors per exportar!", "Sense proveidors", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else
            {
                fs = new FileStream(filename, System.IO.FileMode.OpenOrCreate);
                sw = new StreamWriter(fs);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<proveidors xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"proveidors.xsd\">");

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    sw.WriteLine("  <proveidor>");
                    sw.WriteLine("      <codi>" + row[0] + "</codi>");
                    sw.WriteLine("      <nom>" + row[1] + "</nom>");
                    sw.WriteLine("      <adreça>" + row[2] + "</adreça>");
                    sw.WriteLine("      <poblacio>" + row[3] + "</poblacio>");
                    sw.WriteLine("      <cp>" + row[4] + "</cp>");
                    sw.WriteLine("  </proveidor>");
                }
                sw.WriteLine("</proveidors>");
                sw.Close();
                fs.Close();
            }
        }
        #endregion


        private int ObtenirId()
        {
            int id;
            da = new OdbcDataAdapter("SELECT @@identity FROM ccomanda", cn);
            ds = new DataSet();
            da.Fill(ds);

            id = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            return id;
        }

        private void ActualitzarArticleRebut(int codiCom, string codiArt)
        {//La connexio ja esta oberta
            OdbcCommand cmd = new OdbcCommand();

            cmd.Connection = cn;
            cmd.CommandText = "UPDATE dcomanda set rebut=true where codicomanda=" + codiCom + " AND codiarticle =  '" + codiArt + "';";

            cmd.ExecuteNonQuery();

        }

        private int StockActual(string codiarticle)
        {
            int stock;
            da = new OdbcDataAdapter("SELECT estoc FROM article WHERE codi='" + codiarticle + "';", cn);
            ds = new DataSet();
            da.Fill(ds);

            stock = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
            return stock;
        }

        private void AfegirStock(string codiarticle, int afegir)
        {
            int stockAfegit;
            OdbcCommand cmd = new OdbcCommand();

            stockAfegit = StockActual(codiarticle) + afegir;

            cmd.Connection = cn;
            cmd.CommandText = "UPDATE article set estoc=" + stockAfegit + " where codi='" + codiarticle + "';";

            cmd.ExecuteNonQuery();
        }

        private bool ValidarXML(string xmlFile, string xsdFile)
        {

            bool isValid = false;
            XmlReaderSettings settings = new XmlReaderSettings();
            try
            {
                settings.Schemas.Add(null, xsdFile);
                settings.ValidationType = ValidationType.Schema;
                XmlDocument document = new XmlDocument();
                document.Load(xmlFile);
                XmlReader rdr = XmlReader.Create(new StringReader(document.InnerXml), settings);

                while (rdr.Read())
                {

                }
                isValid = true;
            }
            catch { }
            return isValid;
        }

        private bool ExisteixArticle(string codiArticle)
        {
            bool existeix = true;
            da = new OdbcDataAdapter("SELECT * FROM article WHERE codi='" + codiArticle +"'", cn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) existeix = false;
            return existeix;
        }

        private bool ExisteixProveidor(string codiProveidor)
        {
            bool existeix = true;
            da = new OdbcDataAdapter("SELECT * FROM proveidor WHERE codi='" + codiProveidor + "'", cn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) existeix = false;
            return existeix;
        }

        private bool ExisteixComanda(int codiComanda)
        {
            bool existeix = true;
            da = new OdbcDataAdapter("SELECT * FROM ccomanda WHERE codi=" + codiComanda, cn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) existeix = false;
            return existeix;
        }

        private bool ExisteixArticleEnComanda(string codiArticle, int codiComanda)
        {
            bool existeix = true;
            da = new OdbcDataAdapter("SELECT * FROM dcomanda WHERE codicomanda=" + codiComanda + " and codiarticle='" + codiArticle + "'", cn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count == 0) existeix = false;
            return existeix;
        }

        /*Errors*/
        private void CrearArxiuDerrors()
        {
            System.IO.FileStream fs = new FileStream("errors.xml", System.IO.FileMode.Create);
            System.IO.StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<errors xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"articles.xsd\">");
            sw.WriteLine("</errors>");
            sw.Close();
            fs.Close();

        }

        private void AfegirError(string proces, string descripcio)
        {
            const string FITXERERROR = "errors.xml";

            DateTime ara = DateTime.Now;
            XmlDocument docError;
            XmlNode root;
            XmlElement elem;
            XmlElement subElement1, subElement2, subElement3;

            //Crear Arxiu d'errors si no existeix
            if (!System.IO.File.Exists(FITXERERROR)) CrearArxiuDerrors();

            docError = new XmlDocument();
            docError.Load("errors.xml");
            
            root = docError.DocumentElement;
            elem = docError.CreateElement("error");

            //Subnodes
            subElement1 = docError.CreateElement("proces");
            subElement1.InnerText = proces;

            subElement2 = docError.CreateElement("data");
            subElement2.InnerText = Convert.ToString(ara);

            subElement3 = docError.CreateElement("descripcio");
            subElement3.InnerText = descripcio;

            elem.AppendChild(subElement1);
            elem.AppendChild(subElement2);
            elem.AppendChild(subElement3);
            root.AppendChild(elem);

            docError.Save("errors.xml");
        }

    }
}


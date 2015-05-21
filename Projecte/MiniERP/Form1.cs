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
        private void frmERP_Load(object sender, EventArgs e)
        {

        }

        private void exportarToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void valoracióStockToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void articlesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportarArticles();
        }

        private void proveïdorsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImportarProveidors();
        }


        //Methods
        private void ImportarArticles()
        {
            const string RUTA = "articles.xml";

            string codi;
            string descripcio;
            int stock;
            int preu;
            XmlDocument xml;
            XmlNodeList xnList;
            OdbcCommand cmd;

            if (ValidateXML("articles.xml", "articles.xsd"))
            {
                cn.Open(); //Obrir el access

                xml = new XmlDocument();
                xml.Load(RUTA);
                xnList = xml.SelectNodes("/articles/article");

                cmd = new OdbcCommand();
                cmd.Connection = cn;

                #region Insertar articles
                foreach (XmlNode xn in xnList)
                {
                    //Obtenir les dades
                    codi = xn["codi"].InnerText;
                    descripcio = xn["descripcio"].InnerText;
                    stock = Convert.ToInt32(xn["estoc"].InnerText);
                    preu = Convert.ToInt32(xn["preu"].InnerText);

                    //Insert
                    cmd.CommandText = "INSERT INTO article VALUES ('" + codi + "','" + descripcio + "'," + stock + "," + preu + ");";
                    cmd.ExecuteNonQuery();
                }
                #endregion

                cn.Close(); //Tencar el access
                MessageBox.Show("Importació realitzada correctament", "Importacio correcta");
            }
            else MessageBox.Show("FITXER XML D'ARTICLES NO VÀLID", "Error de validació", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ImportarProveidors()
        {
            string RUTA = "proveidors.xml";

            string codi;
            string nom;
            string direccio;
            string poble;
            string cPostal;
            XmlDocument xml;
            XmlNodeList xnList;
            OdbcCommand cmd;

            if (ValidateXML("proveidors.xml", "proveidors.xsd"))
            {
                cn.Open(); //Obrir access

                xml = new XmlDocument();
                xml.Load(RUTA);
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

                    //les guardem a access
                    cmd.CommandText = "INSERT INTO proveidor VALUES ('" + codi + "','" + nom + "','" + direccio + "','" + poble + "','" + cPostal + "');";
                    cmd.ExecuteNonQuery();
                }
                #endregion

                cn.Close(); //Tencar el access
                MessageBox.Show("Importació realitzada correctament", "Importacio correcta");
            }
            else MessageBox.Show("FITXER XML D'ARTICLES NO VÀLID", "Error de validació", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool ValidateXML(string xmlFile, string xsdFile)
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

    }
}


using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;


namespace Verimatik
{
    // VERİMATİK SINIFI 4 ANA VERİ İŞLEMİNİ SİZİN YERİNİZE YAPAR. (KAYDET, GÜNCELLE, SİL , LİSTELE)
    // MODEL NESNESI UYUMLUDUR.
    // VERİ KÜMESİNİ JSON FORMATINA DÖNÜŞTÜRME ÖZELLİĞİ MEVCUTTUR.

    

    // DİKKAT ! TABLONUZDAKİ İLK ALAN " ID " ŞEKLİNDE TANIMLANMALIDIR.
    // AKSİ HALDE BU SINIF İÇERİSİNDE İLGİLİ ALANLARI KENDİ TERCİHİNİZE GÖRE DEĞİŞTİRMELİSİNİZ.

    ///<summary>
    /// VM Sınıfı veritabanı bağlantısını sağlar.
    ///</summary>
    public  class VM
    {
     
        static SqlConnection cn;

        private static string cnstring =""; // Bağlantı bilgilerinizi tanımlayınız


        /// <summary>
        ///  Kaydet methodu  adı belirtilen tabloya ArrayList formatındaki kolon değerlerini kaydeder
        /// tüm kolonların, veritabanındaki diziliş sırası ile ArrayList nesnesine eklenmesi gerekir.
        /// </summary>
        /// <param name="TabloAdi">Kayıt İşlemi yapılacak tablonun adı</param>
        /// <param name="KolonDegerleri">Veritabanı Tablosunun kolon değerleri (Array List nesnesi)</param>
        /// <returns>Kayıt işleminin sonucu  olarak Kayıt ID değeri geri gönderilir.</returns>
        public static int Kaydet(string TabloAdi, ArrayList KolonDegerleri)
        {
            try
            {
      
                if (TabloAdi != "" && KolonDegerleri.Count > 0)
                {
                    SqlCommand cmd;
                    ArrayList Kolonlar = Listele_TabloKolonlariniGetir(TabloAdi);
                    string Parametre = ""; // VALUES Cümleciğinden sonraki her bir parametre için tanımlandı
                    TabloAdi = TabloSemaAdiGetir(TabloAdi) + "." + TabloAdi;
                    string SqlCumlecigi = "INSERT " + TabloAdi + " (";
                    string SqlDegerCumlecigi = " VALUES(";


                    cmd = new SqlCommand();

                    #region Parametre metini oluşturuluyor ve SqlCommand nesnesinin parametrelerine değeri ile beraber ekleniyor
                    for (int i = 0; i < KolonDegerleri.Count; i++)
                    {


                        if (i > 0)
                        {
                            SqlCumlecigi += ",";
                            SqlDegerCumlecigi += ",";
                        }

                        Parametre = "@" + i.ToString(); // (@DEGISKEN) ifadesi oluşturuluyor. (@0, @1 ...) gibi...
                        SqlCumlecigi += Kolonlar[i].ToString();// Kolon Adları oluşturuluyor.
                        SqlDegerCumlecigi += Parametre;
                        cmd.Parameters.AddWithValue(Parametre, KolonDegerleri[i].ToString());



                    }
                    #endregion

                    SqlCumlecigi += ") " + SqlDegerCumlecigi + ")"; // Sql Cümleciği VALUES(@Degisken)<- kapanma parantezi ile tamamlanıyor...
                    SqlCumlecigi += "\n SELECT @ID=@@IDENTITY";
                    cmd.CommandText = SqlCumlecigi;

                    SqlParameter IDParameter = new SqlParameter("@ID", SqlDbType.Int);
                    IDParameter.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(IDParameter);
                    ExecuteNonQuery(cmd);


                    return (int)IDParameter.Value;

                }
                else
                {
                    return 0;
                }



            }
            catch (Exception ex)
            {

                return 0;
            }
        }

        /// <summary>
        ///  Kaydet methodunun bu versiyonu Kayıt işleminin yapılacağı ilgili tablonun Model Class nesnesini parametre olarak alır.
        /// </summary>
        /// <param name="TabloModelNesnesi"> Tablo Model Class nesnesi</param>
        /// <returns>Kayıt işlemi başarılı olursa Kayıt ID değerini geri gönderir. Aksi halde SIFIR döner</returns>
        public static int Kaydet(Object TabloModelNesnesi)
        {
            try
            {
                #region Değişken Tanımlamaları

                SqlCommand cmd = new SqlCommand();
                System.Reflection.PropertyInfo[] KolonListesi = TabloModelNesnesi.GetType().GetProperties();//Model Nesnesinin property listesi alınıyor

                string TabloAdi = TabloModelNesnesi.GetType().Name;//Model Class nesnesinin adı, aynı zamanda veritabanındaki tablo adına karşılık gelir
                TabloAdi = TabloSemaAdiGetir(TabloAdi) + "." + TabloAdi;
                string Parametre = ""; // VALUES Cümleciğinden sonraki her bir parametre için tanımlandı
                string SqlCumlecigi = "INSERT " + TabloAdi + " (";
                string SqlDegerCumlecigi = " VALUES(";
                string KolonDegeri = "";
                #endregion

                #region Parametre metini oluşturuluyor ve SqlCommand nesnesinin parametrelerine değeri ile beraber ekleniyor
                for (int i = 0; i < KolonListesi.Length; i++)
                {



                    if (KolonListesi[i].Name != "ID")
                    {
                        if (i > 1)
                        {
                            SqlCumlecigi += ",";
                            SqlDegerCumlecigi += ",";
                        }

                        Parametre = "@" + KolonListesi[i].Name; // (@DEGISKEN) ifadesi oluşturuluyor.
                        SqlCumlecigi += KolonListesi[i].Name;// Kolon Adları oluşturuluyor.
                        SqlDegerCumlecigi += Parametre;

                        // String tipinde bir nesneye değer atanmamışsa null hatası almamak için boş değer atıyoruz...
                        if (KolonListesi[i].PropertyType.Name == "String" && KolonListesi[i].GetValue(TabloModelNesnesi, null) == null)
                        {
                            KolonDegeri = "";
                        }
                        else
                        {
                            KolonDegeri = KolonListesi[i].GetValue(TabloModelNesnesi, null).ToString();
                        }


                        cmd.Parameters.AddWithValue(Parametre, KolonDegeri);
                    }




                }
                #endregion

                #region Sql İfadesi tamamlanıp, çalıştırılıyor...

                SqlCumlecigi += ") " + SqlDegerCumlecigi + ")"; // Sql Cümleciği VALUES(@Degisken)<- kapanma parantezi ile tamamlanıyor...
                SqlCumlecigi += "\n SELECT @ID=@@IDENTITY";
                cmd.CommandText = SqlCumlecigi;

                SqlParameter IDParameter = new SqlParameter("@ID", SqlDbType.Int);
                IDParameter.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(IDParameter);
                ExecuteNonQuery(cmd);

                #endregion

                return (int)IDParameter.Value;


            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        ///  Guncelle methodu adı belirtilen tabloya ArrayList formatındaki kolon değerlerini kaydeder
        ///  tüm kolonların, veritabanındaki diziliş sırası ile ArrayList nesnesine eklenmesi gerekir.
        /// </summary>
        /// <param name="TabloAdi">Güncelleme İşlemi yapılacak tablonun adı</param>
        /// <param name="KolonDegerleri">Veritabanı Tablosunun kolon değerleri</param>
        /// <param name="ID">Veritabanı Tablosunun ID değeri</param>
        /// <returns>Kayıt işleminin sonucu mesaj olarak geri gönderilir.</returns>
        public static string Guncelle(string TabloAdi, ArrayList KolonDegerleri, int ID)
        {
            try
            {

                if (TabloAdi != "" && KolonDegerleri.Count > 0)
                {
                    SqlCommand cmd; ;
                    string Parametre = ""; // VALUES Cümleciğinden sonraki her bir parametre için tanımlandı
                    TabloAdi = TabloSemaAdiGetir(TabloAdi) + "." + TabloAdi;
                    string SqlCumlecigi = "UPDATE " + TabloAdi + " SET ";
                    cmd = new SqlCommand();
                    ArrayList Kolonlar = Listele_TabloKolonlariniGetir(TabloAdi);
                    for (int i = 0; i < KolonDegerleri.Count; i++)
                    {


                        if (i > 0)
                        {
                            SqlCumlecigi += ",";
                        }
                        Parametre = "@" + i.ToString();
                        SqlCumlecigi += " " + Kolonlar[i].ToString() + "=" + Parametre + " ";
                        cmd.Parameters.AddWithValue(Parametre, KolonDegerleri[i].ToString());


                    }

                    SqlCumlecigi += " WHERE ID=" + ID;
                    cmd.CommandText = SqlCumlecigi;
                    ExecuteNonQuery(cmd);
                    return "Kayıt Güncellendi";
                }
                else
                {
                    return "Kayıt işleminde Eksik yada geçersiz parametre bulunmaktadır!";
                }

            }
            catch (Exception)
            {

                return "Güncelleme hatası oluştu!";
            }
        }

        /// <summary>
        ///  Guncelle methodunun bu versiyonu Model Nesnesi olarak belirtilmiş tabloya Guncelleme işlemi yapar
        /// </summary>
        /// <param name="TabloModelNesnesi">Tablo Model Nesnesi</param>
        /// <returns>İşlem sonucu metin ifadesi(string) olarak geri döner</returns>
        public static string Guncelle(Object TabloModelNesnesi)
        {
            try
            {
                #region Değişken Tanımlamaları

                SqlCommand cmd = new SqlCommand();
                System.Reflection.PropertyInfo[] KolonListesi = TabloModelNesnesi.GetType().GetProperties();//Model Nesnesinin property listesi alınıyor

                string TabloAdi = TabloModelNesnesi.GetType().Name;//Model Class nesnesinin adı, aynı zamanda veritabanındaki tablo adına karşılık gelir
                TabloAdi = TabloSemaAdiGetir(TabloAdi) + "." + TabloAdi;
                string Parametre = ""; // VALUES Cümleciğinden sonraki her bir parametre için tanımlandı
                string SqlCumlecigi = "UPDATE " + TabloAdi + " SET ";
                string KolonDegeri = "";
         

                #endregion

                #region Parametre metini oluşturuluyor ve SqlCommand nesnesinin parametrelerine değeri ile beraber ekleniyor
                for (int i = 0; i < KolonListesi.Length; i++)
                {



                    if (KolonListesi[i].Name != "ID")
                    {
                        if (i > 1)
                        {
                            SqlCumlecigi += ",";

                        }

                        // String tipinde bir nesneye değer atanmamışsa null hatası almamak için boş değer atıyoruz...
                        if (KolonListesi[i].PropertyType.Name == "String" && KolonListesi[i].GetValue(TabloModelNesnesi, null) == null)
                        {
                            KolonDegeri = "";
                        }
                        else
                        {
                            KolonDegeri = KolonListesi[i].GetValue(TabloModelNesnesi, null).ToString();
                        }

                        Parametre = "@" + KolonListesi[i].Name; // (@DEGISKEN) ifadesi oluşturuluyor.
                        SqlCumlecigi += " " + KolonListesi[i].Name + "=" + Parametre + " ";
                        cmd.Parameters.AddWithValue(Parametre, KolonDegeri);
                    }




                }
                #endregion

                #region Sql İfadesi tamamlanıp, çalıştırılıyor...

                SqlCumlecigi += " WHERE ID=" + KolonListesi[0].GetValue(TabloModelNesnesi, null);
                cmd.CommandText = SqlCumlecigi;
                ExecuteNonQuery(cmd);
                return "Kayıt Güncellendi";

                #endregion




            }
            catch (Exception ex)
            {
                return "Güncelleme hatası oluştu!";
            }
        }

        /// <summary>
        ///  Sil methodu adı belirtilen tablodan  ID değerine karşılık gelen kayıdı siler
        /// </summary>
        /// <param name="TabloAdi"> İşlem yapılacak Tablonun adı</param>
        /// <param name="Id">İşlem yapılacak Kayıdın  ID değeri</param>
        /// <returns> İşlem sonucu metin mesajı olarak geri döner</returns>
        public static string Sil(string TabloAdi, int Id)
        {
            try
            {
                if (TabloAdi != "" && Id > 0)
                {

                    SqlCommand cmd = new SqlCommand();
                    TabloAdi = TabloSemaAdiGetir(TabloAdi) + "." + TabloAdi;
                    string SqlCumlecigi = "DELETE FROM " + TabloAdi + " WHERE Id=@Id";
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.CommandText = SqlCumlecigi;
                    ExecuteNonQuery(cmd);
                    return "Kayıt silindi";

                }
                else
                {
                    return "Silme işlemi için eksik ya da geçersiz parametre belirtilmiş.";
                }

            }
            catch (Exception ex)
            {

                return "Silme işlemi hatası oluştu !";
            }
        }

        /// <summary>
        /// Sil methodunun bu versiyonu  Model Nesnesi olarak belirtilmiş tablodan kayıt siler.
        /// </summary>
        /// <param name="TabloModelNesnesi">Tablo Model Nesnesi</param>
        /// <returns>İşlem sonucu metin ifadesi(string) olarak geri döner</returns>
        public static string Sil(Object TabloModelNesnesi)
        {
            try
            {

                SqlCommand cmd = new SqlCommand();
                System.Reflection.PropertyInfo[] KolonListesi = TabloModelNesnesi.GetType().GetProperties();//Model Nesnesinin property listesi alınıyor
                string TabloAdi = TabloModelNesnesi.GetType().Name;//Model Class nesnesinin adı, aynı zamanda veritabanındaki tablo adına karşılık gelir
                TabloAdi = TabloSemaAdiGetir(TabloAdi) + "." + TabloAdi;
                int ID = int.Parse(KolonListesi[0].GetValue(TabloModelNesnesi, null).ToString());
                string SqlCumlecigi = "DELETE FROM " + TabloAdi + " WHERE ID=@ID";
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.CommandText = SqlCumlecigi;
                ExecuteNonQuery(cmd);
                return "Kayıt silindi";



            }
            catch (Exception ex)
            {

                return "Silme işlemi hatası oluştu !";
            }
        }

        /// <summary>
        /// Listele fonksiyonun bu versiyonu, Sql Sorgu Cümleciği ile Kayıt Listelemenizi sağlar.
        /// </summary>
        /// <param name="SqlCumlecigi">Sorgu ifadesi </param>
        /// <returns>Tablodaki kayıtlar DataTable nesnesi olarak geri gönderilir.</returns>
        public static DataTable Listele(string SqlCumlecigi)
        {

            DataTable dt = new DataTable(); ;
            if (SqlCumlecigi != "")
            {

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = SqlCumlecigi;
                dt = ExecuteReader(cmd);

    
            }
            return dt;


        }



        // Aşağıdaki Listele_TabloKolonlariniGetir fonksiyonu sadece bu sınıf içinde kullanılan özel bir kodlamadır. Dışarıdan erişilemez.
        /// <summary>
        ///  TabloKolonlariniGetir fonksiyonu adı belirtilen tablonun kolon adlarını getirir.
        /// </summary>
        /// <param name="TabloAdi">İlgili Tablo Adı</param>
        /// <returns>Kolon adları Array List formatında geri gönderilir.</returns>
        private static ArrayList Listele_TabloKolonlariniGetir(string TabloAdi)
        {
            try
            {
                #region Kolon Adlarına erişebilmek için tablodan örnek bir kayıt çekiyorum...
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@TABLE AND DATA_TYPE !='uniqueidentifier' AND DATA_TYPE!='timestamp'";
                cmd.Parameters.AddWithValue("@TABLE", TabloAdi);
                DataTable dt = ExecuteReader(cmd);
                ArrayList Kolonlar = new ArrayList();
                string KolonAdi;

                #region Kolonlar doğru sıralayabilmek için bir ArrayList  oluşturuluyor
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    KolonAdi = dt.Rows[k][0].ToString();

                    // Sorgu içerisinde yer almasını İSTEMEDİĞİNİZ kolon adlarını aşağıdaki IF cümleciğinde belirtmelisiniz !
                    #region Listeye eklenmemesi gereken, otomatik değer alan kolonlar

                    // 0 index li eleman TABLO ID değeri olduğundan onu yok sayıyoruz
                    if (k > 0)
                    {
                        Kolonlar.Add(KolonAdi);
                    }

                    #endregion



                }
                return Kolonlar;


                #endregion

                #endregion
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static string TabloSemaAdiGetir(string TabloAdi)
        {
            try
            {
                string SqlCumlecigi = "select TABLE_SCHEMA  from INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + TabloAdi+"'";
                DataTable dt = Listele(SqlCumlecigi);
                return dt.Rows[0][0].ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }

                /// <summary>
        /// ExecuteNonQuery fonksiyonu veritabanında Sorgusuz işlem gerçekleştirir
        /// (INSERT-UPDATE-DELETE)
        /// </summary>
        /// <param name="command">Sql Command nesnesi</param>
        public static void ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                cn = new SqlConnection();
                cn.ConnectionString = cnstring;

                command.Connection = cn;

                //  command.CommandType = CommandType.StoredProcedure;

                using (command)
                {
                    if (cn.State == ConnectionState.Open)
                    {
                        cn.Close();
                    }
                    cn.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cn.Close();
            }
        }

        /// <summary>
        /// Veritabanındaki tablo değerini okur ve Data Table nesnesi olarak tekrar oluşturur
        /// Parametre olarak Sql Command  nesnesi alır.
        /// </summary>
        /// <param name="sqlcmd">Sql Command nesnesi</param>
        /// <returns> DataTable nesnesi döndürür</returns>
        public static DataTable ExecuteReader(SqlCommand sqlcmd)
        {

            cn = new SqlConnection(cnstring);

            DataTable dt = new DataTable();
            DataColumn dc;
            DataRow dr;

            try
            {

                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }

                cn.Open();
                sqlcmd.Connection = cn;



                SqlDataReader sqldr = sqlcmd.ExecuteReader();

                //Get columns from database then apply them to datatable.
                for (int i = 0; i < sqldr.FieldCount; i++)
                {
                    dc = new DataColumn();
                    dc.ColumnName = sqldr.GetName(i);
                    dc.DataType = sqldr.GetFieldType(i);
                    dt.Columns.Add(dc);
                }

                //Filling to Rows from database.
                while (sqldr.Read())
                {
                    dr = dt.NewRow();
                    for (int j = 0; j < sqldr.FieldCount; j++)
                    {
                        dr[j] = sqldr[j];
                    }
                    dt.Rows.Add(dr);
                }
                //Close datareader.
                sqldr.Close();


                //Return a Table as Sql Table
                return dt;


            }

            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                cn.Close();
            }

        }

               /// <summary>
       /// JsonDonustur fonksiyonu DataTable nesnesini Javascript Object Notasyonuna yani JSON a çevirir
       /// </summary>
       /// <param name="dt">DataTable Nesnesi</param>
       /// <returns>Json sonucunu string değişkeni olarak döndürür</returns>
        public static string JsonaDonustur(DataTable dt)
        {
            try
            {
                string json = "{";


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i > 0)
                    {
                        json += ",";
                    }

                    json += "{";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j > 0)
                        {
                            json += ",";
                        }
                        json += '"' + dt.Columns[j].ColumnName + '"' + ":" + '"' + dt.Rows[i][j] + '"';
                    }
                    json += "}";
                }

                json += "}";

                return json;
            }
            catch (Exception)
            {

                throw;
            }
        }



    }
}

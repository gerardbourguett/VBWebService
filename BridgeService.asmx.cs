using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Web.Services;
using Newtonsoft.Json;

namespace VBWebService
{
    /// <summary>
    /// Descripción breve de BridgeService
    /// </summary>
    [WebService(Namespace = "http://www.tempuri.org")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    //[System.Web.Script.Services.ScriptService]
    public class BridgeService : System.Web.Services.WebService
    {

        [WebMethod]
        public DataSet SearchPatente(string patente)
        {
            DataSet ds = new DataSet();
            Conexion.Conectar();

            Conexion.adaptador = new SqlDataAdapter("SELECT * FROM patentes WHERE patente = '" + patente + "'", Conexion.conexion);
            Conexion.adaptador.Fill(ds);
            return ds;
        }

        [WebMethod]
        public DataSet SearchAllPatents()
        {
            DataSet ds = new DataSet();
            Conexion.Conectar();
            Conexion.adaptador = new SqlDataAdapter("SELECT * FROM patentes", Conexion.conexion);
            Conexion.adaptador.Fill(ds);
            return ds;
        }

        [WebMethod]
        public string AddPatent(string caja, string patente)
        {
            try
            {
                int codigoError;
                //Agregar la patente a la base de datos
                AddLogEntry(caja, patente);

                //Llamar a la API de JsonPlaceholder
                var postId = CreatePatent(caja, patente, out codigoError);

                return postId;
            }
            catch (Exception ex)
            {
                return "Error al agregar la patente: " + ex.Message;
            }
        }

        public string AddLogEntry(string caja, string patente)
        {
            string postId = "";
            int codigoError;

            try
            {
                postId = CreatePatent(caja, patente, out codigoError);
            }
            catch (Exception ex)
            {
                return "Error al agregar la patente: " + ex.Message;
            }

            Conexion.Conectar();

            using (SqlCommand cmd = new SqlCommand("z_wsCarabineros_RegistraLog", Conexion.conexion))
            {
                // Establecer el tipo de comando como procedimiento almacenado
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Definir los parámetros del procedimiento almacenado
                cmd.Parameters.Add("@caja", System.Data.SqlDbType.NVarChar, 10).Value = caja;
                cmd.Parameters.Add("@patente", System.Data.SqlDbType.NVarChar, 10).Value = patente;
                cmd.Parameters.Add("@fecha", System.Data.SqlDbType.NVarChar, 10).Value = DateTime.Now.ToString("yyyy-MM-dd"); ;
                cmd.Parameters.Add("@hora", System.Data.SqlDbType.NVarChar, 5).Value = DateTime.Now.ToString("HH:mm"); ;
                cmd.Parameters.Add("@codigo_error", System.Data.SqlDbType.Int).Value = codigoError;
                cmd.Parameters.Add("@status", System.Data.SqlDbType.NVarChar, 100).Value = postId;

                // Parámetro de salida
                SqlParameter salidaParam = new SqlParameter("@salida", System.Data.SqlDbType.Int);
                salidaParam.Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add(salidaParam);

                try
                {
                    Conexion.conexion.Open();
                    cmd.ExecuteNonQuery();
                    int salida = (int)cmd.Parameters["@salida"].Value;
                    Conexion.conexion.Close();
                    return salida.ToString();
                }
                catch (Exception ex)
                {
                    Conexion.conexion.Close();
                    return "Error al agregar la patente: " + ex.Message;
                }
            }
        }

        private string CreatePatent(string title, string body, out int codigoError)
        {
            try
            {
                var client = new HttpClient();
                var post = new
                {
                    title = title,
                    body = body,
                };
                var postContent = new StringContent(JsonConvert.SerializeObject(post), Encoding.UTF8, "application/json");

                var response = client.PostAsync("https://jsonplaceholder.typicode.com/posts", postContent).Result;

                codigoError = (int)response.StatusCode;

                response.EnsureSuccessStatusCode();

                var responseBody = response.Content.ReadAsStringAsync().Result;

                //Procesamos el contenido de la respuesta de la API
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                string postId = jsonResponse.id;

                return $"StatusCode: {codigoError}, PostId: {postId}";
            }
            catch (HttpRequestException ex)
            {
                // Manejo de errores específicos de HTTP
                throw new Exception("Error en la solicitud HTTP: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                throw new Exception("Error al crear la publicación: " + ex.Message);
            }
        }

        [WebMethod]
        public string Envio(string caja, string patente)
        {
            string message = "";
            string vFecha = DateTime.Now.ToString("yyyy-MM-dd");
            string vHora = DateTime.Now.ToString("HH:mm");
            string vcaja = caja;
            string vPatente = patente;

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://jsonplaceholder.typicode.com/posts"),
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        title = "Envio de patente",
                        body = "Caja: " + vcaja + " Patente: " + vPatente + " Fecha: " + vFecha + " Hora: " + vHora,
                        codigo_error = 0,
                        status = "OK"
                    }), Encoding.UTF8, "application/json")
                };
                message = request.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                message = "Error al enviar la patente: " + ex.Message;
                throw new Exception("Error al enviar la patente: " + ex.Message);
            }

            return message;
        }
    }
}

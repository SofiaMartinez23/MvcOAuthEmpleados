using MvcOAuthEmpleados.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace MvcOAuthEmpleados.Services
{
    public class ServiceEmpleados
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue Header;
        private IHttpContextAccessor contextAccessor;

        public ServiceEmpleados(IConfiguration configuration,
            IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
            this.UrlApi = configuration.GetValue<string>
            ("ApiUrls:ApiEmpleados");
            this.Header = new
            MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<string> GetTokenAsync
        (string userName, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    UserName = userName,
                    Password = password
                };
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent
                (json, Encoding.UTF8, "application/json");
                HttpResponseMessage reponse =
                await client.PostAsync(request, content);
                if (reponse.IsSuccessStatusCode)
                {
                    string data = await reponse.Content
                    .ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("reponse").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage reponse =
                await client.GetAsync(request);
                if (reponse.IsSuccessStatusCode)
                {
                    T data = await reponse.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        //VAMOS A REALIZAR UNA SOBRECARGA DEL METODO 
        //RECIBIENDO EL TOKEN 
        private async Task<T> CallApiAsync<T>
    (string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage reponse =
                await client.GetAsync(request);
                if (reponse.IsSuccessStatusCode)
                {
                    T data = await reponse.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            string request = "api/empleados";
            List<Empleado> empleados = await
            this.CallApiAsync<List<Empleado>>(request);
            return empleados;
        }

        //ALMACENAREMOS EL TOKEN EN SESSION 
        //POR AHORA, RECIBIREMOS EL TOKEN EN EL METODO 
        public async Task<Empleado> FindEmpleadoAsync
    (int id)
        {
            string token = this.contextAccessor.HttpContext.User
                .FindFirst(z => z.Type == "TOKEN").Value;
            string request = "api/empleados/" + id;
            Empleado empleado = await
            this.CallApiAsync<Empleado>(request, token);
            return empleado;
        }

        public async Task<Empleado> GetPerfilAsync()
        {
            string token = this.contextAccessor.HttpContext.User
               .FindFirst(z => z.Type == "TOKEN").Value;
            string request = "api/empleados/perfil";
            Empleado empleado = await this.CallApiAsync<Empleado>(request, token);
            return empleado;
        }

        public async Task<List<Empleado>> GetCompisAsync()
        {
            string token = this.contextAccessor.HttpContext.User
               .FindFirst(z => z.Type == "TOKEN").Value;
            string request = "api/empleados/compis";
            List<Empleado> empleado = await this.CallApiAsync<List<Empleado>>(request, token);
            return empleado;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            string request = "api/empleados/oficios";
            List<string> oficios = await this.CallApiAsync<List<string>>(request);
            return oficios;
        }


        private string TranformCollectionToQuery(List<string> collection)
        {
            string result = "";
            foreach (string item in collection)
            {
                result += "oficio=" + item + "&";
            }
            result = result.TrimEnd('&');
            return result;
        }


        public async Task<List<Empleado>> GetEmpleadosOficiosAsync(List<string> oficios)
        {
            string request = "api/empleados/empleadosoficios";
            string data = this.TranformCollectionToQuery(oficios);

            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request + "?" + data);
            return empleados;
        }

        public async Task UpdateEmpleadosOficiosAsync(int incremento, List<string> oficios)
        {
            string request = "api/empleados/incrementosalario/" + incremento;
            string data = this.TranformCollectionToQuery(oficios);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage reponse = await client.PutAsync(request + "?" + data, null);
            }
        }
    }
}

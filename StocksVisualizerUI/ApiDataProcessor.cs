using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace StocksVisualizerUI
{
    public static class ApiDataProcessor
    {
        public async static Task<List<T>> Get_API_Obj_List<T>(string url)
        {
            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    List<T> data = await response.Content.ReadAsAsync<List<T>>();
                    return data;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }

        }

        public async static Task<T> Get_API_Obj<T>(string url)
        {
            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }

        }

        public async static Task<Dictionary<T, U>> Get_API_Obj_Dict<T, U>(string url)
        {

            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsAsync<Dictionary<T, U>>();
                    return data;
                    //var list = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                    //foreach (var temp in list)
                    //{
                    //   var company = JsonConvert.DeserializeObject<IEXStock> (temp.Value.ToString());
                    //}
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }

        }

    }

}

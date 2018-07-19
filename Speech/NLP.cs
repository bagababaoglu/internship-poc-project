using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithmia;
using Json;
using Newtonsoft.Json;

namespace Speech
{
    class NLP
    {

        public static int Main()
        {
            var input = "{"
            + "  \"src\": \"Open toplama menu.\","
            + "  \"format\": \"tree\","
            + "  \"language\": \"english\""
            + "}";
            var client = new Client("-----");
            var algorithm = client.algo("deeplearning/Parsey/1.1.0");
            var response = algorithm.pipeJson<object>(input);
            string output = JsonConvert.SerializeObject(response.result);
            object stuff = JsonConvert.DeserializeObject(output);
            // Console.WriteLine(response.result);
            // Console.WriteLine(output);
            Console.WriteLine((String)response.result);
            Console.WriteLine((String)stuff);
            //Console.WriteLine(x.ToString());


            Console.ReadLine();

            return 0;
        }


    }
}

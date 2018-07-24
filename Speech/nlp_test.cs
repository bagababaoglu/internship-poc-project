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
    class nlp_test
    {
      
    public static int Main()
        {
            var input = "{"
            + "  \"src\": \"Toplama menüsünü aç\","
            + "  \"format\": \"tree\","
            + "  \"language\": \"turkish\""
            + "}";




            var client = new Client("simb6Fvv21PsbZaQU0wGvOdXF4N1");
            var algorithm = client.algo("deeplearning/Parsey/1.1.0");
            AlgorithmResponse response = algorithm.pipeJson<object>(input);
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

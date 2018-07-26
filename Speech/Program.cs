using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleCloudSamples;
using System.Collections;
using System.IO;
using Google.Cloud.Language.V1;
using Google.Protobuf.Collections;
using Newtonsoft.Json;

namespace Speech
{
    class Program
    {
        private  string[] commands;
        private  string[] menus;
        private  string [][] list;
        private  string[] columns;
        private  String lang;
        private  int currentMenuIndex=-1;
        private  ArrayList verbs = new ArrayList();
        private  String nouns = "";
        private String input = "";
        List<data> _data = new List<data>();
        

        private  bool immidiate_action = false;
        public  int openMenu(String command, bool checkImmediate) //This function is for opening a menu.
        {
            int cmp;
            ArrayList menu_index = new ArrayList();
            menu_index = checkMenus(command);
            if (menu_index.Count == 0)
            {
                Console.WriteLine("We could not find a menu.");
                return 1;
            }
            else
            {
                int immidiate_menu_index = -1;
                if (checkImmediate)
                {

                    foreach (int i in menu_index)
                    {
                        String[] command_items = command.Split(' ');

                        foreach (String command_item in command_items)
                        {
                            int start_index = menus[i].IndexOf(command_item, StringComparison.CurrentCultureIgnoreCase);

                            if (command_item.Length == menus[i].Length && start_index == 0)
                            {
                                immidiate_menu_index = i;
                                immidiate_action = true;
                            }

                        }
                        

                    }

                }



                if (immidiate_action)
                {
                    Console.WriteLine("I opened the " + menus[immidiate_menu_index] + " menu");
                    immidiate_action = false;
                    currentMenuIndex = immidiate_menu_index;
                }
                else
                {
                    foreach (int i in menu_index)
                    {
                        Console.WriteLine(i + ". Menu: " + menus[i]);

                    }
                    Console.WriteLine("Would you like to open one of the menus above? y/n");
                    String t1 = Console.ReadLine();
                    cmp = String.Compare(t1, "y", ignoreCase: true);
                    if (cmp == 0)
                    {

                        Console.WriteLine("Enter number of menu");
                        String t2 = Console.ReadLine();
                        int j;
                        if (Int32.TryParse(t2, out j))
                        {
                            foreach (int ind in menu_index)
                            {
                                if (ind == j)
                                {
                                    //action required to open the menu.
                                    Console.WriteLine("I opened the " + menus[ind] + " menu");
                                    currentMenuIndex = ind;
                                    return 0;
                                }
                            }
                            Console.WriteLine("You have entered invalid index.");
                            return 1;
                        }

                        else
                        {
                            Console.WriteLine("String could not be parsed.");
                        }

                    }
                    else
                    {
                        Console.WriteLine("See you..");
                        return 0;
                    }
                }



            }



            return 0;
        }
        public  ArrayList checkMenus(String command)
        {

            ArrayList indexs = new ArrayList();
            int i = 0;
            String[] words_in_cmd = null;
            String[] words_in_menuitem = null;
            words_in_cmd = command.Split(' ');


            foreach (String menu_item in menus)
            {

                words_in_menuitem = menu_item.Split(' ');
                foreach (String word in words_in_cmd)
                {
                    foreach (String menu_word in words_in_menuitem)
                    {
                        StringComparison comp = StringComparison.OrdinalIgnoreCase;
                        if (!word.Equals("contains", comp) && menu_word.Contains(word, comp) && indexs.IndexOf(i) < 0 && !word.Contains("menu", comp) && word!="")
                        {

                            indexs.Add(i);


                        }
                    }

                }
                i++;

            }



            return indexs;
        }
        public  int populateList (String lang)
        {
            
            if (currentMenuIndex == -1)
            {
                Console.WriteLine("You haven't open any menu yet.");
                return -1;
            }
           
            try
            {
                int row_count = 0;
                int column_count = 0;
                UTF8Encoding encoding = new UTF8Encoding();
                string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\data\listformenu_" + currentMenuIndex +".txt", encoding);
                int number_of_rows = lines.Length;
                int number_of_columns = lines[0].Split(' ').Length;
                list = new String [number_of_rows][];
                for(int l=0; l < number_of_rows; l++)
                {
                    list[l] =new String[number_of_columns];
                }
                Console.WriteLine("Printing the list..");
                foreach (String line in lines)
                {
                    String [] line_elements = line.Split(' ');
                    
                    foreach (String element in line_elements)
                    {
                        list[row_count][column_count]=element;
                        //row_count + "," + column_count + " " +
                        Console.Write(list[row_count][column_count] + " ");
                        column_count++;

                    }
                    Console.WriteLine("");
                    column_count = 0;
                    row_count++;


                }
                return 0;

            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("We couldnt find a list for this menu.");
            }



            return -1;

          
        }
        public  int getColumns()
        {

            UTF8Encoding encoding = new UTF8Encoding();
            try
            {
                columns = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\data\columnforlist_" + currentMenuIndex + "_" + lang + ".txt", encoding)[0].Split(' ');
               
                return 0;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("We couldn't find a column information for this list.");
                return -1;
            }
           




            
        }
       public  ArrayList findColumn(String [] columns ,String command_target)
        {
            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            ArrayList indexs = new ArrayList();
            int i = -1;
            foreach (String column in columns)
            {
                i++;
                if (command_target.Contains(column, comp))
                {
                    indexs.Add(i);
                }
            }

            if (indexs.Count == 0)
            {
                return null;

            }
            else
            {
                return indexs;
            }
         
               
            }
            
        
        public  int filter(String command_target) // This function filters the given list 
            //in terms of the column and reference.
           
            // command template "X listesini filtrele"
        {
            if (populateList(lang) == 0)
            {
                Console.WriteLine("\nI am trying to filter the list");
                if (list == null)
                {
                    Console.WriteLine("We couldn't get the list.");
                    return -1;
                }



                if (getColumns() == -1)
                {
                    return -1;
                }
                ArrayList indexs = findColumn(columns, command_target);
                if(indexs == null)
                {
                    Console.WriteLine("We couldn't find a match");
                    return -1;
                }
                ArrayList row_indexs = new ArrayList();
                StringComparison comp = StringComparison.OrdinalIgnoreCase;
                String reference = "";

                foreach (int index in indexs)
                {
                    command_target = command_target.Replace(columns[index], "");

                }
                reference = command_target;
                Console.WriteLine("Command target is: " + reference);

                //search those columns according to the reference.

                foreach (int index in indexs)
                {
                    for (int s = 1; s < list.Length; s++)
                    {
                        if (reference.Contains(list[s][index], comp) && !row_indexs.Contains(s))
                        {

                            row_indexs.Add(s);


                        }

                    }



                }
                Console.WriteLine("\nFiltered list..\n");
                String[] row = null;
                foreach (int row_index in row_indexs)
                {
                    row = list[row_index];
                    foreach (String row_element in row)
                    {

                        Console.Write(row_element + " ");
                    }
                    Console.WriteLine();

                }

                if (row_indexs.Count == 0)
                {
                    Console.WriteLine("We couldn't find a match");
                }
                return 0;

            }
            else
            {
                return -1;
            }}
        
        public  int implementFunction(int index, String command) //This function is called by checkCommands if it finds a 
                                                                       //match. It splits the command into command_target and command_function. command_function is always same we have
                                                                       //limited amounth of functions. It uses switch case statement to decide which function to implement. And then
                                                                       //implements the function accordinng to command_target. 
        {
            String command_target=command;
            int r = -1;
            switch (lang)  //choosing the language for implementing command.
            {
                case "tr":

                    switch (commands[index])  // choosing a function to implement.
                    {
                        case "ekranını aç":
                            Console.WriteLine("I implemented funtion. " + "ekranını aç");
                            break;
                        case "menüsünü aç":
                            Console.WriteLine("I implemented funtion. " + "menüsünü aç");
                            r=openMenu(command_target, true);
                            break;
                        case "filtrele":
                            Console.WriteLine("I implemented funtion. " + "filtrele");
                            r =filter(command_target);
                       
                            break;
                        case "login":
                            Console.WriteLine("I implemented funtion. " + "login");
                            break;
                       
                        default:
                            Console.WriteLine("Default");
                            break;
                    }


                    break;



                case "en":

                    switch (commands[index])
                    {
                        case ("open"):
                            Console.WriteLine("I implemented funtion. " + "open");
                            r = openMenu(command_target, true);
                            break;
                        case ("list"):
                        case ("get"):
                        case ("bring"):
                        case ("filter"):
                        case ("search"):
                            Console.WriteLine("I implemented funtion. " + "filter");
                            r = filter(command_target);                            
                            break;

                        default:
                            Console.WriteLine("Command is not recognized.");
                            break;
                    }

                    break;

                default:
                    Console.WriteLine("Language is not recognized.");
                    break;
            }
            Console.WriteLine("Are you satisfied with the service? y/n");
            String s = Console.ReadLine();

            //for indexing.
            switch (s)
            {
                case ("y"):
                    addIndex(index);
                    break;
                case ("n"):
                    Console.WriteLine("We will try to improve our service. Thank you for your comment.");
                    break;
                default:
                    Console.WriteLine("Invalid input.");
                    break;
            }
           

            return r;
        }
        public class data
        {
            public string _command { get; set; }
            public ArrayList _verbs { get; set; }
            public string _nouns { get; set; }
            
            public int _index { get; set; }

        }
        public int populateFrequentCommands()

        {
            try
            {
                UTF8Encoding encoding = new UTF8Encoding();
                String json = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + @"\data\frequent_commands.json", encoding);
                if (json.Contains("_command"))
                {
                    data[] temp = JsonConvert.DeserializeObject<data[]>(json);
                    _data = temp.ToList<data>();
                    
                }
                
                return 0;

            } catch (Exception e)
            {
                Console.WriteLine("There was a problem reading frequent commands from JSON.");
                return -1;
            }
           

          
        }
        public int addIndex(int index)
        {
            if (_data ==null)
            {
                Console.WriteLine("Problem while adding an index. There is no list.");
                return -1;
            }
            data temp = new data()
            {
                _command = input,
                _verbs = verbs,
                _nouns = nouns,
                _index = index

            };
            Boolean isExist=false;
           
            foreach (data d in _data)
            {
                if (d._command.Equals(temp._command) && d._nouns.Equals(temp._nouns) && d._verbs.Count==temp._verbs.Count)
                {
                    isExist = true;
                    break;
                }
            }

            if (isExist)
            { 
                Console.WriteLine("This data already exist in the frequent commands.");
            }
            else
            {
                _data.Add(temp);
            }
            return 0;
        }
        public int checkFrequentCommands(String command)
        {
            foreach(data d in _data)
            {
                if (d._command.Equals(command))
                {
                    Console.WriteLine("I found your command in the frequent commands.");
                    verbs = d._verbs;
                    nouns = d._nouns;
                    input = command;
                    implementFunction(d._index, nouns);
                    
                    return 0;

                }

            }


            return -1;
        }
        public  int checkCommands(String command) //this function is for checking the existing commands 
                                                        //to match the given command. If it finds a match it implements the function if not ask the user input.
        {
            int ch=checkFrequentCommands(command);
            if (ch == 0)
            {
                return 0;
            }
            int z=getVerbs(command);
            int l=getNouns(command);
            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            if (z==-1 || l == -1)
            {
                Console.WriteLine("There was a problem analyzing the text");
                return -1;
            }

            if (verbs.Count != 0)
            {
                ArrayList indexs = new ArrayList();
                int cmp;
                int index = -1;
                int counter = 1;
                input = command;

                foreach (String actual_command in commands)
                {

                    index++;
                    foreach (String verb in verbs)
                    {
                        if (verb.Contains(actual_command, comp))
                        {
                            indexs.Add(index);
                            
                        }

                    }
                    
                }


                if (indexs.Count != 0)
                {
                    foreach (int i in indexs)
                    {
                        Console.WriteLine("Command is : " + command + ". That is corresponds to: " + commands[i] + " in system");

                        if (Recognize.IsReliable)
                        {
                            
                            cmp = 0;
                            Recognize.IsReliable = false;

                        }
                        else
                        {
                            Console.WriteLine("Do you want to continue? y/n"); 
                            String t3 = Console.ReadLine();
                            cmp = String.Compare(t3, "y", ignoreCase: true);

                        }

                        if (cmp == 0)
                        {
                            implementFunction(i, nouns);
                           
                        }
                        else
                        {
                            Console.WriteLine("Do you want to give it another try? y/n");
                            String t = Console.ReadLine();
                            cmp = String.Compare(t, "y", ignoreCase: true);
                            if (cmp == 0)
                            {
                                implementCommand(getCommand(lang));
                            }
                            else
                            {
                                Console.WriteLine("See you..");
                            }
                            return 0;
                        }


                    }

                }
                else
                {
                    if (command.Contains("contains", comp) && command.Contains("open", comp))
                    {

                        String search_word = nouns;
                        openMenu(search_word, false);
                    }
                    else
                    {
                        Recognize.IsReliable = false;
                        Console.WriteLine("We couldn't match your command.");
                        if (Recognize.Alternatives.Count != 0)
                        {
                            Console.WriteLine("Did you mean one of these? y/n");
                            int count_alternative = 0;
                            foreach (String alternative in Recognize.Alternatives)
                            {
                                Console.WriteLine(count_alternative + ". " + alternative);
                                count_alternative++;
                            }
                            String t5 = Console.ReadLine();
                            cmp = String.Compare(t5, "y", ignoreCase: true);

                            if (cmp == 0)
                            {
                                Console.WriteLine("Please enter the number.");
                                t5 = Console.ReadLine();
                                int k;
                                if (Int32.TryParse(t5, out k) && k < Recognize.Alternatives.Count)
                                {
                                    checkCommands((String)Recognize.Alternatives[k]);
                                }
                                else
                                {
                                    Console.WriteLine("You have entered an invalid index.");

                                }


                            }
                            else
                            {
                                Console.WriteLine("Trying to find a menu..");
                                openMenu(command, true);
                            }


                        }else
                        {
                            Console.WriteLine("There are no alternatives for your command.");
                        }
                    }
                   

                }


                counter++;


                return -1;
            }
            return -1;
        }
       

       
        public  int implementCommand(ArrayList word_list) //This function is called after getting a command from user. 
                                                                //If user approves it calls checkCommands to check existing commands. Else it asks user to try again.
        {

            int cmp;
            String t = "y";



            if (Recognize.IsReliable)
            {
                cmp = 0;

            }
            else
            {
                //Console.WriteLine("We detect " + word_list.Count + " commands. Do you want to continue? y/n");
                //t = Console.ReadLine();
                //t = "y";
                //cmp = String.Compare(t, "y", ignoreCase: true);
                cmp = 0;
            }


            if (cmp == 0)
            {

                foreach (String command in word_list)
                {
                    
                    checkCommands(command);
                }
            }
            else
            {
                cmp = String.Compare(t, "n", ignoreCase: true);
                if (cmp == 0)
                {
                    Console.WriteLine("Do you want to give it another try? y/n");
                    t = Console.ReadLine();
                    cmp = String.Compare(t, "y", ignoreCase: true);
                    if (cmp == 0)
                    {
                        implementCommand(getCommand(lang));
                    }
                    else
                    {
                        Console.WriteLine("See you..");
                    }
                    return 0;
                }
                else
                {
                    Console.WriteLine("You have given unavailable input.");
                    return 1;
                }
            }


            // Console.WriteLine("Command implemented succesfully.");
            return 0;
        }

        public  ArrayList getCommand(String l) //This function calls googles api to get a verbal command from user.
        {
            ArrayList word_list = new ArrayList();
            Task<object> task = Recognize.StreamingMicRecognizeAsync(5, l);
            task.Wait();
            Object result = task.Result;
            word_list = (ArrayList)result;

            return word_list;
        }

       public  int getVerbs(String command)
        {
            RepeatedField<Token> tokens = Analyze.AnalyzeSyntaxFromText(command);
            verbs = new ArrayList();

            foreach (var token in tokens)
            {
               
                if ((int)token.PartOfSpeech.Tag != 6)
                {
                    verbs.Add(token.Text.Content);
                }

            }
            if (verbs.Count == 0)
            {
                return -1;
            }else
            {
                return 0;
            }
          
        }
        public  int getNouns(String command)
        {
            RepeatedField<Token> tokens = Analyze.AnalyzeSyntaxFromText(command);
            nouns = "";
            foreach (var token in tokens)
            {

                if ((int)token.PartOfSpeech.Tag == 1 || (int)token.PartOfSpeech.Tag == 6 || (int)token.PartOfSpeech.Tag == 11)
                {
                    nouns=nouns+token.Text.Content+" ";
                }

            }
            if (nouns == null)
            {
                return -1;
            }else
            {
                return 0;
            }}

        public int search (ArrayList items, String search_item)
        {
            


            return 0;
        }
        public static int Main(string[] args)
        {
            Program p = new Program();
            p.lang = "en";
            //error check
            if (p.populateCommands()==-1 || p.populateMenus() == -1 || p.populateFrequentCommands()==-1)
            {
                Console.WriteLine("Program terminated.");
                return -1;
                
            }
            //test cases
            ArrayList word_list = new ArrayList();
            word_list.Add("Toplama menüsünü aç.");
            word_list.Add("Hakan çalışanını ara.");
            //Console.WriteLine(word_list);
            p.implementCommand(word_list);
            
            //main program
            while (false)
            {

                word_list = p.getCommand(p.lang);
                if (word_list.Count > 0 && ((String.Compare(((String)word_list[0]), "exit", ignoreCase: true) == 0) ||
                    (String.Compare(((String)word_list[0]), "kapat", ignoreCase: true) == 0)))
                {
                    Console.WriteLine("Program terminated.");
                    break;
                }
                if (word_list.Count > 0)
                {
                    p.implementCommand(word_list);

                }


            }
            //write frequent commands to a file.
            string json = JsonConvert.SerializeObject(p._data.ToArray());
            System.IO.File.WriteAllText(Directory.GetCurrentDirectory() + @"\data\frequent_commands.json", json);
            Console.ReadLine();

            return 0;


        }
        public  int populateCommands() //This function called at the beginning of the program to populate commands 
                                              //into arraylist that are currently stored in text file.
        {
            UTF8Encoding encoding = new UTF8Encoding();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\data\commands_" + lang + ".txt", encoding);
                commands = lines;
                return 0;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("We couldn't find available commands.");
                return -1;
            }
           
            

        }
        public  int populateMenus() //This function called at the beginning of the program to populate menus 
                                           //into arraylist that are currently stored in text file.
        {
            UTF8Encoding encoding = new UTF8Encoding();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\data\menus_" + lang + ".txt", encoding);
                menus = lines;
                return 0;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("We couldn't find available menus.");
                return -1;
            }
            
        }
    }
}

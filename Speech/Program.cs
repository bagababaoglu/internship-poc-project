using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleCloudSamples;
using System.Collections;
using System.IO;


namespace Speech
{
    class Program
    {
        private static string[] commands;
        private static string[] menus;
        private static string[][] list;
        private static string[] columns;
        private static String lang;
        private static int currentMenuIndex = -1;

        private static bool immidiate_action = false;
        public static int openMenu(String command, bool checkImmediate) //This function is for opening a menu.
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
                        int start_index = menus[i].IndexOf(command, StringComparison.CurrentCultureIgnoreCase);

                        if (command.Length == menus[i].Length && start_index == 0)
                        {
                            immidiate_menu_index = i;
                            immidiate_action = true;
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
        public static ArrayList checkMenus(String command)
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
                        if (!word.Equals("içeren", comp) && menu_word.Contains(word, comp) && indexs.IndexOf(i) < 0)
                        {

                            indexs.Add(i);


                        }
                    }

                }
                i++;

            }



            return indexs;
        }
        public static int populateList(String lang)
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
                string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\data\listformenu_" + currentMenuIndex + ".txt", encoding);
                int number_of_rows = lines.Length;
                int number_of_columns = lines[0].Split(' ').Length;
                list = new String[number_of_rows][];
                for (int l = 0; l < number_of_rows; l++)
                {
                    list[l] = new String[number_of_columns];
                }
                Console.WriteLine("Printing the list..");
                foreach (String line in lines)
                {
                    String[] line_elements = line.Split(' ');

                    foreach (String element in line_elements)
                    {
                        list[row_count][column_count] = element;
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
            catch (FileNotFoundException)
            {
                Console.WriteLine("We couldnt find a list for this menu.");
            }



            return -1;


        }
        public static int getColumns()
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
        public static ArrayList findColumn(String[] columns, String command_target)
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


        public static int filter(String command_target) // This function filters the given list 
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
                if (indexs == null)
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
            }






        }
        public static int implementFunction(int index, String command) //This function is called by checkCommands if it finds a 
                                                                       //match. It splits the command into command_target and command_function. command_function is always same we have
                                                                       //limited amounth of functions. It uses switch case statement to decide which function to implement. And then
                                                                       //implements the function accordinng to command_target. 
        {
            String command_target;
            //String command_function;
            int i = command.IndexOf(commands[index]);
            int old_length = command.Length;
            command_target = command.Substring(0, (old_length - (old_length - i)) - 1);
            //command_function= command.Substring(i,old_length-i);
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
                            r = openMenu(command_target, true);
                            break;
                        case "filtrele":
                            Console.WriteLine("I implemented funtion. " + "filtrele");
                            r = filter(command_target);

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
                            Console.WriteLine("I implemented funtion. " + "menüsünü aç");
                            r = openMenu(command_target, true);
                            break;
                        case ("filter"):
                            Console.WriteLine("I implemented funtion. " + "filtrele");
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


            return r;
        }
        public static int checkCommands(String command) //this function is for checking the existing commands 
                                                        //to match the given command. If it finds a match it implements the function if not ask the user input.
        {
            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            if (command.Length != 0)
            {
                ArrayList indexs = new ArrayList();
                int cmp;
                int index = -1;
                int counter = 1;


                foreach (String actual_command in commands)
                {

                    index++;

                    if (command.Contains(actual_command, comp))
                    {
                        indexs.Add(index);
                    }
                }


                if (indexs.Count != 0)
                {
                    foreach (int i in indexs)
                    {
                        Console.WriteLine(counter + ". Command is : " + command + ". That is corresponds to: " + commands[i] + " in system");

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
                            implementFunction(i, command);

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
                    if (command.Contains("içeren", comp) && command.Contains("menüyü aç", comp))
                    {
                        int end_index = command.IndexOf("içeren", comp);
                        String search_word = command.Substring(0, (command.Length - (command.Length - end_index)) - 1);
                        openMenu(search_word, false);
                    }
                    else
                    {
                        Recognize.IsReliable = false;
                        Console.WriteLine("We couldn't match your command.");
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


                    }

                }


                counter++;


                return -1;
            }
            return -1;
        }



        public static int implementCommand(ArrayList word_list) //This function is called after getting a command from user. 
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
                Console.WriteLine("We detect " + word_list.Count + " commands. Do you want to continue? y/n");
                t = Console.ReadLine();
                cmp = String.Compare(t, "y", ignoreCase: true);
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

        public static ArrayList getCommand(String l) //This function calls googles api to get a verbal command from user.
        {



            ArrayList word_list = new ArrayList();
            Task<object> task = Recognize.StreamingMicRecognizeAsync(5, l);
            task.Wait();
            Object result = task.Result;
            word_list = (ArrayList)result;

            return word_list;
        }

        public static int Main(string[] args)
        {

            Console.WriteLine("Please enter a valid language code.");
            lang = Console.ReadLine();
            if (lang == "")
            {
                Console.WriteLine("Error");
                return 1;
            }



            if (populateCommands() == -1 || populateMenus() == -1)
            {
                
                Console.WriteLine("Program terminated.");
                return -1;
            }


            ArrayList word_list = new ArrayList();
            word_list.Add("Toplama open menu");
            word_list.Add("çalışanlar name Hakan filter");
            implementCommand(word_list);

            while (true)
            {

                //Console.WriteLine("Press any key for giving a command.");
                //String t = Console.ReadLine();
                word_list = getCommand(lang);

                if (word_list.Count > 0 && ((String.Compare(((String)word_list[0]), "exit", ignoreCase: true) == 0) ||
                    (String.Compare(((String)word_list[0]), "kapat", ignoreCase: true) == 0)))
                {
                    Console.WriteLine("Program terminated.");
                    break;
                }
                if (word_list.Count > 0)
                {
                    implementCommand(word_list);

                }


            }


            Console.ReadLine();

            return 0;


        }
        public static int populateCommands() //This function called at the beginning of the program to populate commands 
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
        public static int populateMenus() //This function called at the beginning of the program to populate menus 
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

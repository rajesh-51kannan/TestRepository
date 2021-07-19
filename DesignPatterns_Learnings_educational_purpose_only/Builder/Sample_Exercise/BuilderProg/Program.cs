using System;
using System.Collections.Generic;
using System.Text;

namespace BuilderProg
{
    class Program
    {
        static void Main(string[] args)
        {
            //Improper implementation of builder 
            //ExecuteBrokenBuilder();

            //builder for html
            var builder = new HtmlBuilder("main");

            //non-fluent method
            builder.AddChild("div", "builder");
            builder.AddChild("div", "Sample");
            Console.Write(builder.ToString());

            builder.Clear();


            //fluent method
            builder.AddChildFluent("div", "builder")
                    .AddChildFluent("div", "fluent sample")
                    .AddChildFluent("span", "added a span");


            //fluent with recursive geenerics
            var me = Person.New.Called("Kannan").WorkAsA("developer").Build();


            Console.Write(builder.ToString());

            Console.WriteLine(me);

            Console.ReadKey();
        }


        private static void ExecuteBrokenBuilder()
        {
            var hello = "hello";
            var sb = new StringBuilder();
            sb.Append("<div>");
            sb.Append(hello);
            sb.Append("</div>");

            Console.WriteLine(sb);

            var words = new[] { "builder", "Sample" };

            sb.Clear();
            sb.Append("<main>");
            foreach (var word in words)
            {
                sb.AppendFormat("<div>{0}</div>", word);
            }
            sb.Append("</main>");

            Console.WriteLine(sb);

        }

        private static void GetAdultsMeal()
        {
            Console.WriteLine("Adults order:");



            Console.WriteLine("Add a beer after the meal");

            Console.WriteLine("Your adults meal is ready based on the above ordered items. Kindly pick the meal to your table:");

        }

        private static void GetKidsMeal()
        {
            Console.WriteLine("Kids order:");



            Console.WriteLine("Prepare starter for kids");
            Console.WriteLine("Prepare main food for kids");
            Console.WriteLine("Prepare dessert for kids");

            Console.WriteLine("Add some chocolates");

            Console.WriteLine("Your kids meal is ready based on the above ordered items. Kindly pick the meal to your table:");


        }

    }
}

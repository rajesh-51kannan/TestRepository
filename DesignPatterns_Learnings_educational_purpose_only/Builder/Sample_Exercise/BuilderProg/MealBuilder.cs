using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuilderProg
{
    public class KidsMealBuilder : IMealBuilder
    {
        public void AddDessert()
        {
            Console.WriteLine("Prepare dessert for adults");

        }

        public void AddMain()
        {
            Console.WriteLine("Prepare main food for adults");

        }

        public void AddStarter()
        {
            Console.WriteLine("Prepare starter for adults");

        }

        public void GetMeal()
        {
            throw new NotImplementedException();
        }
    }

    public interface IMealBuilder
    {
        void AddStarter();
        void AddMain();
        void AddDessert();
        void GetMeal();
    }
}

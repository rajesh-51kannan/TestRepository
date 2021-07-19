namespace AdapterPattern
{
    public class VectorOfFloat<D> : Vector<VectorOfFloat<D>, float, D>
        where D : IInteger, new()
    {

        public VectorOfFloat()
        {

        }

        public VectorOfFloat(params float[] values) : base(values)
        {

        }

        public static VectorOfFloat<D> operator +
            (VectorOfFloat<D> lhs, VectorOfFloat<D> rhs)
        {
            var result = new VectorOfFloat<D>();
            var dim = new D().Value;

            for (int i = 0; i < dim; i++)
            {
                result[i] = lhs[i] + rhs[i];
            }

            return result;
        }
    }
}

namespace AdapterPattern
{
    public class VectorOfInt<TSelf, D> : Vector<TSelf, int, D>
        where D : IInteger, new()
        where TSelf : Vector<TSelf, int, D>, new()
    {

    }
}

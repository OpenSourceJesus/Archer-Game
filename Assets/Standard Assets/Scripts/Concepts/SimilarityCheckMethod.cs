public class SimilarityCheckMethod<T1, T2>
{
    public delegate float GetSimilarity(T1 obj1, T2 obj2);
    public event GetSimilarity getSimilarity;
}
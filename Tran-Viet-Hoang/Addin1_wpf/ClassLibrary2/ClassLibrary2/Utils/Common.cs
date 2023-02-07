namespace ClassLibrary2.Utils
{
    public static class Common
    {
        public static double Max(double num1, double num2)
        {
            double max = 0;
            if (num1 <= num2)
            {
                max = num2;
            }
            else
            {
                max = num1;
            }
            return max;
        }
    }
}
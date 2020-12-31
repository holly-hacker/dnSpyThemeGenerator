namespace dnSpyThemeGenerator.Converters
{
    internal interface IThemeConverter<in TFrom, in TTo>
    {
        void CopyTo(TFrom source, TTo donor);
    }
}
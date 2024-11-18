namespace Markcons.Extensions
{
    public static class ExceptionExtensions
    {
        public static ErroOr<T> ToErroOrFailure<T>(this Exception ex, string code)
        {
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            return ErroOr<T>.Failed($"Código: {code}, Erro: {errorMessage}");
        }
    }
}

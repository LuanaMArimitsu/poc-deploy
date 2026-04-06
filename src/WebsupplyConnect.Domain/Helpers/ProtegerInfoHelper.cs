namespace WebsupplyConnect.Domain.Helpers
{
    public static class ProtegerInfoHelper
    {

        public static string ProtegerTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return telefone;

            var numeros = new string(telefone.Where(char.IsDigit).ToArray());

            // Se for pequeno demais, mascara tudo
            if (numeros.Length <= 8)
                return new string('*', numeros.Length);

            var primeiros = numeros.Substring(0, 6);
            var ultimos = numeros.Substring(numeros.Length - 2, 2);
            var meioMascarado = new string('*', numeros.Length - 8);

            return $"{primeiros}{meioMascarado}{ultimos}";
        }

        public static string ProtegerEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return email;

            var indexArroba = email.IndexOf('@');

            // Email inválido ou sem @
            if (indexArroba <= 0)
                return "***";

            var parteUsuario = email.Substring(0, indexArroba);
            var dominio = email.Substring(indexArroba);

            var quantidadeVisivel = Math.Min(5, parteUsuario.Length);
            var visivel = parteUsuario.Substring(0, quantidadeVisivel);
            var mascarado = new string('*', parteUsuario.Length - quantidadeVisivel);

            return $"{visivel}{mascarado}{dominio}";
        }
    }
}

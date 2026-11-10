using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Migrations.Scripts
{
    public static class ScriptRunner
    {
        private static readonly Regex SeparadorGo = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void EjecutarScript(this MigrationBuilder migrationBuilder, string nombreArchivo)
        {
            foreach (var lote in SeparadorGo.Split(LeerScript(nombreArchivo)))
            {
                if (!string.IsNullOrWhiteSpace(lote))
                    migrationBuilder.Sql(lote);
            }
        }

        private static string LeerScript(string nombreArchivo)
        {
            var ensamblado = typeof(ScriptRunner).Assembly;
            var recurso = ensamblado.GetManifestResourceNames().SingleOrDefault(n => n.EndsWith("." + nombreArchivo, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"No se encontró el script embebido '{nombreArchivo}'. " + "Verifica que esté en Migrations/Scripts y marcado como EmbeddedResource.");

            using var stream = ensamblado.GetManifestResourceStream(recurso)!;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}

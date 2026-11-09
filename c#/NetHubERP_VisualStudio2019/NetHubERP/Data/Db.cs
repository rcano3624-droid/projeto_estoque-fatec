using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NetHubERP.Data
{
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                var item = ConfigurationManager.ConnectionStrings["NetHubERP"];
                if (item == null) throw new InvalidOperationException("Conexão NetHubERP ausente do App.config.");
                return item.ConnectionString;
            }
        }

        public static SqlConnection Open()
        {
            var c = new SqlConnection(ConnectionString);
            try { c.Open(); return c; }
            catch { c.Dispose(); throw; }
        }

        public static void Check()
        {
            using (var c = Open())
            using (var q = new SqlCommand("SELECT COUNT(*) FROM dbo.Usuarios", c))
                q.ExecuteScalar();
        }

        public static SqlCommand Command(SqlConnection c, SqlTransaction tr, string sql)
        {
            var q = new SqlCommand(sql, c);
            if (tr != null) q.Transaction = tr;
            q.CommandTimeout = 30;
            return q;
        }
        public static void Add(SqlCommand q, string name, SqlDbType type, object value, int size = 0)
        {
            var p = size != 0 ? q.Parameters.Add(name, type, size) : q.Parameters.Add(name, type);
            p.Value = value ?? DBNull.Value;
        }
        public static string Str(SqlDataReader r, string name)
        {
            var i = r.GetOrdinal(name);
            return r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i));
        }
        public static int Int(SqlDataReader r, string name) { return Convert.ToInt32(r[name]); }
        public static void RequireAdmin(Models.UserSession u)
        {
            if (u == null || !u.Admin) throw new UnauthorizedAccessException("Apenas administradores podem executar esta operação.");
        }
    }
}

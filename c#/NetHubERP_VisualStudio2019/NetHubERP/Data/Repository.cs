using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using NetHubERP.Models;

namespace NetHubERP.Data
{
    /// <summary>ADO.NET com parâmetros e transações. Não requer NuGet.</summary>
    public static class Repository
    {
        private static void Password(string password, out byte[] salt, out byte[] digest)
        {
            salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            using (var pbkdf = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
                digest = pbkdf.GetBytes(32);
        }
        private static bool VerifyPassword(string password, byte[] salt, byte[] expected)
        {
            byte[] digest;
            using (var pbkdf = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
                digest = pbkdf.GetBytes(32);
            int difference = digest.Length ^ expected.Length;
            for (int i = 0; i < Math.Min(digest.Length, expected.Length); i++) difference |= digest[i] ^ expected[i];
            return difference == 0;
        }
        private static void Audit(SqlConnection c, SqlTransaction tr, int uid, string type, string description)
        {
            using (var q = Db.Command(c, tr, "INSERT INTO dbo.HistoricoAuditoria(UsuarioId,Tipo,Descricao) VALUES(@u,@t,@d)"))
            {
                Db.Add(q, "@u", SqlDbType.Int, uid);
                Db.Add(q, "@t", SqlDbType.NVarChar, type, 40);
                Db.Add(q, "@d", SqlDbType.NVarChar, description, 600);
                q.ExecuteNonQuery();
            }
        }
        private static void AddProductParams(SqlCommand q, Product p)
        {
            Db.Add(q, "@codigo", SqlDbType.NVarChar, p.Codigo, 50);
            Db.Add(q, "@nome", SqlDbType.NVarChar, p.Nome, 180);
            Db.Add(q, "@marca", SqlDbType.NVarChar, p.Marca ?? "", 100);
            Db.Add(q, "@categoria", SqlDbType.NVarChar, p.Categoria ?? "Outros", 80);
            Db.Add(q, "@local", SqlDbType.NVarChar, p.Local ?? "", 120);
            Db.Add(q, "@qtd", SqlDbType.Int, p.Quantidade);
            Db.Add(q, "@min", SqlDbType.Int, p.Minimo);
            var price = q.Parameters.Add("@preco", SqlDbType.Decimal);
            price.Precision = 18; price.Scale = 2; price.Value = p.Preco;
            Db.Add(q, "@foto", SqlDbType.VarBinary, p.Foto, -1);
        }
        private static void Movement(SqlConnection c, SqlTransaction tr, int productId, int userId,
            string type, int qty, int after, string reason, int? clientId, int? supplierId)
        {
            using (var q = Db.Command(c, tr, @"INSERT dbo.MovimentacoesEstoque
                (ProdutoId,UsuarioId,Tipo,Quantidade,SaldoApos,Motivo,ClienteId,FornecedorId)
                VALUES (@p,@u,@t,@q,@s,@m,@c,@f)"))
            {
                Db.Add(q, "@p", SqlDbType.Int, productId);
                Db.Add(q, "@u", SqlDbType.Int, userId);
                Db.Add(q, "@t", SqlDbType.VarChar, type, 10);
                Db.Add(q, "@q", SqlDbType.Int, qty);
                Db.Add(q, "@s", SqlDbType.Int, after);
                Db.Add(q, "@m", SqlDbType.NVarChar, reason ?? "", 100);
                Db.Add(q, "@c", SqlDbType.Int, clientId);
                Db.Add(q, "@f", SqlDbType.Int, supplierId);
                q.ExecuteNonQuery();
            }
        }

        public static bool HasUsers()
        {
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, "SELECT COUNT(*) FROM dbo.Usuarios"))
                return Convert.ToInt32(q.ExecuteScalar()) > 0;
        }
        public static void CreateFirstAdmin(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || password.Length < 8)
                throw new ArgumentException("Informe nome, e-mail e senha de pelo menos oito caracteres.");
            byte[] salt, hash;
            Password(password, out salt, out hash);
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    using (var count = Db.Command(c, tr, "SELECT COUNT(*) FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)"))
                        if (Convert.ToInt32(count.ExecuteScalar()) != 0)
                            throw new InvalidOperationException("A configuração inicial já foi realizada.");
                    using (var q = Db.Command(c, tr, @"INSERT dbo.Usuarios(Nome,Email,Perfil,SenhaHash,SenhaSalt)
                        VALUES (@n,@e,'admin',@h,@s)"))
                    {
                        Db.Add(q, "@n", SqlDbType.NVarChar, name.Trim(), 120);
                        Db.Add(q, "@e", SqlDbType.NVarChar, email.Trim().ToLowerInvariant(), 180);
                        Db.Add(q, "@h", SqlDbType.VarBinary, hash, 32);
                        Db.Add(q, "@s", SqlDbType.VarBinary, salt, 16);
                        q.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }
        public static UserSession Authenticate(string email, string password)
        {
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, @"SELECT Id,Nome,Email,Perfil,SenhaHash,SenhaSalt
                FROM dbo.Usuarios WHERE Email=@email AND Ativo=1"))
            {
                Db.Add(q, "@email", SqlDbType.NVarChar, (email ?? "").Trim().ToLowerInvariant(), 180);
                using (var r = q.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    byte[] hash = (byte[])r["SenhaHash"], salt = (byte[])r["SenhaSalt"];
                    if (!VerifyPassword(password ?? "", salt, hash)) return null;
                    return new UserSession { Id = Db.Int(r, "Id"), Nome = Db.Str(r, "Nome"),
                        Email = Db.Str(r, "Email"), Perfil = Db.Str(r, "Perfil") };
                }
            }
        }
        public static void RegisterLogin(UserSession actor)
        {
            using (var c = Db.Open())
                Audit(c, null, actor.Id, "Acesso", "Entrou no sistema");
        }

        public static List<Product> Products()
        {
            var results = new List<Product>();
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, @"SELECT Id,Codigo,Nome,Marca,Categoria,LocalEstoque,
                Quantidade,EstoqueMinimo,Preco,Foto,Ativo FROM dbo.Produtos WHERE Ativo=1 ORDER BY Nome"))
            using (var r = q.ExecuteReader())
                while (r.Read())
                    results.Add(new Product {
                        Id = Db.Int(r, "Id"), Codigo = Db.Str(r, "Codigo"), Nome = Db.Str(r, "Nome"),
                        Marca = Db.Str(r, "Marca"), Categoria = Db.Str(r, "Categoria"),
                        Local = Db.Str(r, "LocalEstoque"), Quantidade = Db.Int(r, "Quantidade"),
                        Minimo = Db.Int(r, "EstoqueMinimo"), Preco = (decimal)r["Preco"],
                        Foto = r["Foto"] == DBNull.Value ? null : (byte[])r["Foto"], Ativo = (bool)r["Ativo"]
                    });
            return results;
        }

        public static void SaveProduct(Product p, UserSession actor)
        {
            Db.RequireAdmin(actor);
            if (p == null || string.IsNullOrWhiteSpace(p.Codigo) || string.IsNullOrWhiteSpace(p.Nome))
                throw new ArgumentException("Código e nome são obrigatórios.");
            if (p.Quantidade < 0 || p.Minimo < 0 || p.Preco < 0) throw new ArgumentException("Números não podem ser negativos.");
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    if (p.Id == 0)
                    {
                        using (var q = Db.Command(c, tr, @"INSERT dbo.Produtos(Codigo,Nome,Marca,Categoria,LocalEstoque,Quantidade,EstoqueMinimo,Preco,Foto)
                            OUTPUT INSERTED.Id VALUES(@codigo,@nome,@marca,@categoria,@local,@qtd,@min,@preco,@foto)"))
                        {
                            AddProductParams(q, p);
                            p.Id = Convert.ToInt32(q.ExecuteScalar());
                        }
                        if (p.Quantidade > 0) Movement(c, tr, p.Id, actor.Id, "ENTRADA", p.Quantidade, p.Quantidade, "Estoque inicial", null, null);
                        Audit(c, tr, actor.Id, "Cadastro", "Produto cadastrado: " + p.Nome + " (" + p.Quantidade + " un.)");
                    }
                    else
                    {
                        int oldQty;
                        using (var q = Db.Command(c, tr, "SELECT Quantidade FROM dbo.Produtos WITH (UPDLOCK,HOLDLOCK) WHERE Id=@id AND Ativo=1"))
                        {
                            Db.Add(q, "@id", SqlDbType.Int, p.Id);
                            var v = q.ExecuteScalar();
                            if (v == null) throw new InvalidOperationException("Produto não encontrado ou desativado.");
                            oldQty = Convert.ToInt32(v);
                        }
                        using (var q = Db.Command(c, tr, @"UPDATE dbo.Produtos SET Codigo=@codigo,Nome=@nome,Marca=@marca,
                            Categoria=@categoria,LocalEstoque=@local,Quantidade=@qtd,EstoqueMinimo=@min,
                            Preco=@preco,Foto=@foto WHERE Id=@id AND Ativo=1"))
                        {
                            AddProductParams(q, p); Db.Add(q, "@id", SqlDbType.Int, p.Id);
                            q.ExecuteNonQuery();
                        }
                        if (oldQty != p.Quantidade)
                            Movement(c, tr, p.Id, actor.Id, "AJUSTE", Math.Abs(p.Quantidade - oldQty), p.Quantidade,
                                "Ajuste cadastral (" + oldQty + " para " + p.Quantidade + ")", null, null);
                        Audit(c, tr, actor.Id, "Edição", "Produto editado: " + p.Nome +
                            (oldQty != p.Quantidade ? " (quantidade " + oldQty + " para " + p.Quantidade + ")" : ""));
                    }
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }
        public static void DeleteProduct(int id, UserSession actor)
        {
            Db.RequireAdmin(actor);
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction())
            {
                try
                {
                    string name;
                    using (var q = Db.Command(c, tr, "SELECT Nome FROM dbo.Produtos WHERE Id=@id AND Ativo=1"))
                    { Db.Add(q, "@id", SqlDbType.Int, id); name = Convert.ToString(q.ExecuteScalar()); }
                    if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Produto já excluído.");
                    using (var q = Db.Command(c, tr, "UPDATE dbo.Produtos SET Ativo=0 WHERE Id=@id"))
                    { Db.Add(q, "@id", SqlDbType.Int, id); q.ExecuteNonQuery(); }
                    Audit(c, tr, actor.Id, "Exclusão", "Produto removido: " + name);
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }

        public static void Move(int id, bool entry, int amount, string reason, int? clientId, int? supplierId, UserSession actor)
        {
            if (actor == null) throw new UnauthorizedAccessException("Faça login novamente.");
            if (amount <= 0) throw new ArgumentException("Informe uma quantidade inteira maior que zero.");
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    int qty;
                    string name;
                    using (var q = Db.Command(c, tr, @"SELECT Quantidade,Nome FROM dbo.Produtos WITH (UPDLOCK,HOLDLOCK)
                        WHERE Id=@id AND Ativo=1"))
                    {
                        Db.Add(q, "@id", SqlDbType.Int, id);
                        using (var r = q.ExecuteReader())
                        {
                            if (!r.Read()) throw new InvalidOperationException("Produto não encontrado.");
                            qty = Db.Int(r, "Quantidade"); name = Db.Str(r, "Nome");
                        }
                    }
                    if (!entry && amount > qty)
                        throw new InvalidOperationException("Estoque insuficiente. Disponível: " + qty + ".");
                    int after = checked(qty + (entry ? amount : -amount));
                    using (var q = Db.Command(c, tr, "UPDATE dbo.Produtos SET Quantidade=@q WHERE Id=@id"))
                    {
                        Db.Add(q, "@q", SqlDbType.Int, after); Db.Add(q, "@id", SqlDbType.Int, id);
                        q.ExecuteNonQuery();
                    }
                    Movement(c, tr, id, actor.Id, entry ? "ENTRADA" : "SAIDA", amount, after, reason, clientId, supplierId);
                    Audit(c, tr, actor.Id, entry ? "Entrada" : "Saída",
                        (entry ? "+" : "-") + amount + " un. de " + name + " (saldo " + after + ")" +
                        (string.IsNullOrEmpty(reason) ? "" : " - " + reason));
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }

        public static List<Contact> Contacts(bool suppliers)
        {
            string table = suppliers ? "dbo.Fornecedores" : "dbo.Clientes";
            string cols = suppliers ? "Id,Nome,Contato,Telefone,Cidade" : "Id,Nome,Telefone,Email,Cidade";
            var list = new List<Contact>();
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, "SELECT " + cols + " FROM " + table + " WHERE Ativo=1 ORDER BY Nome"))
            using (var r = q.ExecuteReader())
                while (r.Read()) list.Add(new Contact {
                    Id = Db.Int(r, "Id"), Nome = Db.Str(r, "Nome"),
                    Contato = suppliers ? Db.Str(r, "Contato") : "", Telefone = Db.Str(r, "Telefone"),
                    Email = suppliers ? "" : Db.Str(r, "Email"), Cidade = Db.Str(r, "Cidade")
                });
            return list;
        }
        public static void SaveContact(Contact item, bool suppliers, UserSession actor)
        {
            Db.RequireAdmin(actor);
            if (item == null || string.IsNullOrWhiteSpace(item.Nome)) throw new ArgumentException("Informe o nome.");
            string table = suppliers ? "dbo.Fornecedores" : "dbo.Clientes";
            string field = suppliers ? "Contato" : "Email";
            string type = suppliers ? "Fornecedor" : "Cliente";
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction())
            {
                try
                {
                    string sql = item.Id == 0
                        ? "INSERT " + table + "(Nome," + field + ",Telefone,Cidade) OUTPUT INSERTED.Id VALUES (@n,@x,@p,@c)"
                        : "UPDATE " + table + " SET Nome=@n," + field + "=@x,Telefone=@p,Cidade=@c WHERE Id=@id AND Ativo=1";
                    using (var q = Db.Command(c, tr, sql))
                    {
                        Db.Add(q, "@n", SqlDbType.NVarChar, item.Nome.Trim(), 150);
                        Db.Add(q, "@x", SqlDbType.NVarChar, suppliers ? (item.Contato ?? "") : (item.Email ?? ""), suppliers ? 120 : 180);
                        Db.Add(q, "@p", SqlDbType.NVarChar, item.Telefone ?? "", 30);
                        Db.Add(q, "@c", SqlDbType.NVarChar, item.Cidade ?? "", 100);
                        if (item.Id == 0) item.Id = Convert.ToInt32(q.ExecuteScalar());
                        else
                        {
                            Db.Add(q, "@id", SqlDbType.Int, item.Id);
                            if (q.ExecuteNonQuery() != 1) throw new InvalidOperationException(type + " não encontrado.");
                        }
                    }
                    Audit(c, tr, actor.Id, "Cadastro", type + (sql.StartsWith("INSERT") ? " cadastrado: " : " editado: ") + item.Nome);
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }
        public static void DeleteContact(int id, bool suppliers, UserSession actor)
        {
            Db.RequireAdmin(actor);
            string table = suppliers ? "dbo.Fornecedores" : "dbo.Clientes";
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction())
            {
                try
                {
                    string name;
                    using (var q = Db.Command(c, tr, "SELECT Nome FROM " + table + " WHERE Id=@id AND Ativo=1"))
                    { Db.Add(q, "@id", SqlDbType.Int, id); name = Convert.ToString(q.ExecuteScalar()); }
                    if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Registro já removido.");
                    using (var q = Db.Command(c, tr, "UPDATE " + table + " SET Ativo=0 WHERE Id=@id"))
                    { Db.Add(q, "@id", SqlDbType.Int, id); q.ExecuteNonQuery(); }
                    Audit(c, tr, actor.Id, "Exclusão", (suppliers ? "Fornecedor" : "Cliente") + " removido: " + name);
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }

        public static List<Account> Users()
        {
            var list = new List<Account>();
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, "SELECT Id,Email,Nome,Perfil FROM dbo.Usuarios WHERE Ativo=1 ORDER BY Nome"))
            using (var r = q.ExecuteReader())
                while (r.Read()) list.Add(new Account { Id = Db.Int(r, "Id"), Email = Db.Str(r, "Email"),
                    Nome = Db.Str(r, "Nome"), Perfil = Db.Str(r, "Perfil") });
            return list;
        }
        public static void CreateUser(string email, string name, string password, string role, UserSession actor)
        {
            Db.RequireAdmin(actor);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name) || password == null || password.Length < 8)
                throw new ArgumentException("Informe e-mail, nome e senha de no mínimo oito caracteres.");
            if (role != "admin" && role != "funcionario") throw new ArgumentException("Perfil inválido.");
            byte[] salt, hash; Password(password, out salt, out hash);
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction())
            {
                try
                {
                    using (var q = Db.Command(c, tr, @"INSERT dbo.Usuarios(Email,Nome,Perfil,SenhaHash,SenhaSalt)
                        VALUES(@e,@n,@p,@h,@s)"))
                    {
                        Db.Add(q, "@e", SqlDbType.NVarChar, email.Trim().ToLowerInvariant(), 180);
                        Db.Add(q, "@n", SqlDbType.NVarChar, name.Trim(), 120);
                        Db.Add(q, "@p", SqlDbType.VarChar, role, 15);
                        Db.Add(q, "@h", SqlDbType.VarBinary, hash, 32);
                        Db.Add(q, "@s", SqlDbType.VarBinary, salt, 16);
                        q.ExecuteNonQuery();
                    }
                    Audit(c, tr, actor.Id, "Usuário", "Usuário criado: " + email);
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }
        public static void DeleteUser(int id, UserSession actor)
        {
            Db.RequireAdmin(actor);
            if (actor.Id == id) throw new InvalidOperationException("Você não pode excluir a própria conta.");
            using (var c = Db.Open())
            using (var tr = c.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    string email;
                    using (var q = Db.Command(c, tr, "SELECT Email FROM dbo.Usuarios WITH (UPDLOCK,HOLDLOCK) WHERE Id=@id AND Ativo=1"))
                    { Db.Add(q, "@id", SqlDbType.Int, id); email = Convert.ToString(q.ExecuteScalar()); }
                    if (string.IsNullOrEmpty(email)) throw new InvalidOperationException("Usuário não encontrado.");
                    using (var q = Db.Command(c, tr, "UPDATE dbo.Usuarios SET Ativo=0 WHERE Id=@id AND Ativo=1"))
                    { Db.Add(q, "@id", SqlDbType.Int, id); q.ExecuteNonQuery(); }
                    Audit(c, tr, actor.Id, "Usuário", "Usuário removido: " + email);
                    tr.Commit();
                }
                catch { tr.Rollback(); throw; }
            }
        }

        public static List<AuditEntry> History(int limit = 500)
        {
            var list = new List<AuditEntry>();
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, @"SELECT TOP (@max) h.CriadoEm,h.Tipo,h.Descricao,u.Nome AS Usuario
                FROM dbo.HistoricoAuditoria h INNER JOIN dbo.Usuarios u ON u.Id=h.UsuarioId
                ORDER BY h.Id DESC"))
            {
                Db.Add(q, "@max", SqlDbType.Int, limit);
                using (var r = q.ExecuteReader())
                    while (r.Read()) list.Add(new AuditEntry { Data = (DateTime)r["CriadoEm"],
                        Tipo = Db.Str(r, "Tipo"), Usuario = Db.Str(r, "Usuario"), Descricao = Db.Str(r, "Descricao") });
            }
            return list;
        }
        public static List<Movement> Movements(int limit = 200)
        {
            var list = new List<Movement>();
            using (var c = Db.Open())
            using (var q = Db.Command(c, null, @"SELECT TOP (@max) m.CriadoEm,p.Nome AS Produto,m.Tipo,m.Quantidade,
                m.SaldoApos,m.Motivo,u.Nome AS Usuario
                FROM dbo.MovimentacoesEstoque m JOIN dbo.Produtos p ON p.Id=m.ProdutoId
                JOIN dbo.Usuarios u ON u.Id=m.UsuarioId ORDER BY m.Id DESC"))
            {
                Db.Add(q, "@max", SqlDbType.Int, limit);
                using (var r = q.ExecuteReader())
                    while (r.Read()) list.Add(new Movement { Data = (DateTime)r["CriadoEm"],
                        Produto = Db.Str(r, "Produto"), Tipo = Db.Str(r, "Tipo"),
                        Quantidade = Db.Int(r, "Quantidade"), SaldoApos = Db.Int(r, "SaldoApos"),
                        Motivo = Db.Str(r, "Motivo"), Usuario = Db.Str(r, "Usuario") });
            }
            return list;
        }
    }
}

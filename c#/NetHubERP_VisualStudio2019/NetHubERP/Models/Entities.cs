using System;

namespace NetHubERP.Models
{
    public sealed class UserSession
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nome { get; set; }
        public string Perfil { get; set; }
        public bool Admin { get { return Perfil == "admin"; } }
    }
    public sealed class Product
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Marca { get; set; }
        public string Categoria { get; set; }
        public string Local { get; set; }
        public int Quantidade { get; set; }
        public int Minimo { get; set; }
        public decimal Preco { get; set; }
        public byte[] Foto { get; set; }
        public bool Ativo { get; set; }
        public bool Baixo { get { return Quantidade <= Minimo; } }
    }
    public sealed class Contact
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Contato { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Cidade { get; set; }
    }
    public sealed class Account
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nome { get; set; }
        public string Perfil { get; set; }
    }
    public sealed class AuditEntry
    {
        public DateTime Data { get; set; }
        public string Usuario { get; set; }
        public string Tipo { get; set; }
        public string Descricao { get; set; }
    }
    public sealed class Movement
    {
        public DateTime Data { get; set; }
        public string Produto { get; set; }
        public string Tipo { get; set; }
        public int Quantidade { get; set; }
        public int SaldoApos { get; set; }
        public string Motivo { get; set; }
        public string Usuario { get; set; }
    }
}

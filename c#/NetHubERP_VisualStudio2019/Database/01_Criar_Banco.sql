/* NetHub ERP | SQL Server 2016+ | Execute no SSMS em uma instância local.
   Não define usuário/senha padrão. O primeiro administrador será criado pelo programa. */
IF DB_ID(N'NetHubERP') IS NULL
    EXEC(N'CREATE DATABASE [NetHubERP]');
GO
USE NetHubERP;
GO
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(120) NOT NULL,
        Email NVARCHAR(180) NOT NULL,
        Perfil VARCHAR(15) NOT NULL CHECK (Perfil IN ('admin','funcionario')),
        SenhaHash VARBINARY(32) NOT NULL,
        SenhaSalt VARBINARY(16) NOT NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Usuarios_Ativo DEFAULT(1),
        CriadoEm DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_CriadoEm DEFAULT(SYSDATETIME())
    );
    CREATE UNIQUE INDEX UX_Usuarios_Email ON dbo.Usuarios(Email) WHERE Ativo=1;
END;
GO
IF OBJECT_ID(N'dbo.Produtos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Produtos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Codigo NVARCHAR(50) NOT NULL,
        Nome NVARCHAR(180) NOT NULL,
        Marca NVARCHAR(100) NOT NULL DEFAULT(''),
        Categoria NVARCHAR(80) NOT NULL DEFAULT('Outros'),
        LocalEstoque NVARCHAR(120) NOT NULL DEFAULT(''),
        Quantidade INT NOT NULL DEFAULT(0) CHECK (Quantidade >= 0),
        EstoqueMinimo INT NOT NULL DEFAULT(0) CHECK (EstoqueMinimo >= 0),
        Preco DECIMAL(18,2) NOT NULL DEFAULT(0) CHECK (Preco >= 0),
        Foto VARBINARY(MAX) NULL,
        Ativo BIT NOT NULL DEFAULT(1),
        CriadoEm DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())
    );
    CREATE UNIQUE INDEX UX_Produtos_Codigo ON dbo.Produtos(Codigo) WHERE Ativo=1;
END;
GO
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(150) NOT NULL,
        Telefone NVARCHAR(30) NOT NULL DEFAULT(''),
        Email NVARCHAR(180) NOT NULL DEFAULT(''),
        Cidade NVARCHAR(100) NOT NULL DEFAULT(''),
        Ativo BIT NOT NULL DEFAULT(1)
    );
END;
GO
IF OBJECT_ID(N'dbo.Fornecedores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Fornecedores (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(150) NOT NULL,
        Contato NVARCHAR(120) NOT NULL DEFAULT(''),
        Telefone NVARCHAR(30) NOT NULL DEFAULT(''),
        Cidade NVARCHAR(100) NOT NULL DEFAULT(''),
        Ativo BIT NOT NULL DEFAULT(1)
    );
END;
GO
IF OBJECT_ID(N'dbo.MovimentacoesEstoque', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MovimentacoesEstoque (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        ProdutoId INT NOT NULL REFERENCES dbo.Produtos(Id),
        UsuarioId INT NOT NULL REFERENCES dbo.Usuarios(Id),
        Tipo VARCHAR(10) NOT NULL CHECK (Tipo IN ('ENTRADA','SAIDA','AJUSTE')),
        Quantidade INT NOT NULL CHECK (Quantidade > 0),
        SaldoApos INT NOT NULL CHECK (SaldoApos >= 0),
        Motivo NVARCHAR(100) NOT NULL DEFAULT(''),
        ClienteId INT NULL REFERENCES dbo.Clientes(Id),
        FornecedorId INT NULL REFERENCES dbo.Fornecedores(Id),
        CriadoEm DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())
    );
    CREATE INDEX IX_Movimentacoes_Data ON dbo.MovimentacoesEstoque(CriadoEm DESC);
END;
GO
IF OBJECT_ID(N'dbo.HistoricoAuditoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HistoricoAuditoria (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId INT NOT NULL REFERENCES dbo.Usuarios(Id),
        Tipo NVARCHAR(40) NOT NULL,
        Descricao NVARCHAR(600) NOT NULL,
        CriadoEm DATETIME2(0) NOT NULL DEFAULT(SYSDATETIME())
    );
    CREATE INDEX IX_Historico_Data ON dbo.HistoricoAuditoria(CriadoEm DESC);
END;
GO
SELECT 'NetHubERP' AS Banco, 'Tabelas verificadas/criadas' AS Resultado;
GO

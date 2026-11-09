/* EXECUTAR DEPOIS DE 01_Criar_Banco.sql | Não altera dados. */
USE NetHubERP;
GO
DECLARE @faltando NVARCHAR(MAX) = N'';
IF OBJECT_ID('dbo.Usuarios','U') IS NULL SET @faltando += N'Usuarios, ';
IF OBJECT_ID('dbo.Produtos','U') IS NULL SET @faltando += N'Produtos, ';
IF OBJECT_ID('dbo.Clientes','U') IS NULL SET @faltando += N'Clientes, ';
IF OBJECT_ID('dbo.Fornecedores','U') IS NULL SET @faltando += N'Fornecedores, ';
IF OBJECT_ID('dbo.MovimentacoesEstoque','U') IS NULL SET @faltando += N'MovimentacoesEstoque, ';
IF OBJECT_ID('dbo.HistoricoAuditoria','U') IS NULL SET @faltando += N'HistoricoAuditoria, ';
IF @faltando <> N'' THROW 50001, 'Faltam tabelas. Execute o script 01 antes.', 1;
SELECT 'OK' AS Estado, 'Seis tabelas essenciais existentes' AS Verificacao;
SELECT 'Produtos' AS Tabela, COUNT(*) AS Registros FROM dbo.Produtos
UNION ALL SELECT 'Usuarios', COUNT(*) FROM dbo.Usuarios
UNION ALL SELECT 'Clientes', COUNT(*) FROM dbo.Clientes
UNION ALL SELECT 'Fornecedores', COUNT(*) FROM dbo.Fornecedores
UNION ALL SELECT 'MovimentacoesEstoque', COUNT(*) FROM dbo.MovimentacoesEstoque
UNION ALL SELECT 'HistoricoAuditoria', COUNT(*) FROM dbo.HistoricoAuditoria;
SELECT i.name AS Indice, OBJECT_NAME(i.object_id) AS Tabela
FROM sys.indexes i
WHERE i.name IN ('UX_Produtos_Codigo', 'UX_Usuarios_Email', 'IX_Movimentacoes_Data', 'IX_Historico_Data');
GO

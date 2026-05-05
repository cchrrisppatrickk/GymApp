BEGIN TRANSACTION;
GO

CREATE TABLE [ConfiguracionAlertas] (
    [Id] int NOT NULL IDENTITY,
    [HoraEnvio] time NOT NULL,
    [DiasSemana] nvarchar(max) NOT NULL,
    [EnviarNuevosMiembros] bit NOT NULL,
    [EnviarProximosVencimientos] bit NOT NULL,
    [EnviarDeudasPendientes] bit NOT NULL,
    [EnviarPagosHoy] bit NOT NULL,
    [ChatIdDestino] nvarchar(max) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_ConfiguracionAlertas] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260504212438_AddConfiguracionAlerta', N'8.0.22');
GO

COMMIT;
GO


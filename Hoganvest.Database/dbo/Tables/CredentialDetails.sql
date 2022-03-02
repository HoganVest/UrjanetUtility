CREATE TABLE [dbo].[CredentialDetails] (
    [CredentialDetailsId] INT            IDENTITY (1, 1) NOT NULL,
    [CredentialId]        INT            NULL,
    [Account Number]      NVARCHAR (100) NULL,
    [Account Status]      NVARCHAR (100) NULL,
    [PropertyId]          NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([CredentialDetailsId] ASC),
    FOREIGN KEY ([CredentialId]) REFERENCES [dbo].[Credential] ([CredentialId])
);


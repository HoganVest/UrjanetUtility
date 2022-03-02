CREATE TABLE [dbo].[Credential] (
    [CredentialId]   INT            IDENTITY (1, 1) NOT NULL,
    [UserName]       NVARCHAR (100) NULL,
    [CorrelationId]  NVARCHAR (100) NULL,
    [Status]         NVARCHAR (100) NULL,
    [StatusDetail]   NVARCHAR (MAX) NULL,
    [Enabled]        BIT            NULL,
    [ProviderName]   NVARCHAR (100) NULL,
    [LastModified]   DATE           NULL,
    [Created]        DATE           NULL,
    [CreatedBy]      NVARCHAR (100) NULL,
    [LastModifiedBy] NVARCHAR (100) NULL,
    [RunHistory]     BIT            NULL,
    [Mock]           BIT            NULL,
    [Password]       VARCHAR (100)  NULL,
    [website]        VARCHAR (100)  NULL,
    CONSTRAINT [PK_UrjanetCredentials] PRIMARY KEY CLUSTERED ([CredentialId] ASC)
);


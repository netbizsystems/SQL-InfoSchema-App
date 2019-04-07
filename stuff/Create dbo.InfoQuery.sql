﻿USE [SQLInformationStore]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE SCHEMA AES
GO

CREATE TABLE [AES].[InfoQuery](
[QueryId] [uniqueidentifier] NOT NULL,
[QueryName] [nchar](50) NOT NULL,
[QuerySql] [nvarchar](max) NOT NULL,
[QueryOwner] [nchar](50) NOT NULL,
CONSTRAINT [PK_InfoQuery] PRIMARY KEY CLUSTERED ([QueryId] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_InfoQuery_Name] ON [AES].[InfoQuery] ( [QueryName] ASC )
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
GO

CREATE TABLE [AES].[InfoQueryVersion](
[QueryId] [uniqueidentifier] NOT NULL,
[QueryVersion] [int] NOT NULL,
[QuerySql] [nvarchar](max) NOT NULL,
CONSTRAINT [PK_InfoQueryVersion] PRIMARY KEY CLUSTERED ([QueryId] ASC,[QueryVersion] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  ON [PRIMARY] ) ON [PRIMARY]
GO
ALTER TABLE [AES].[InfoQueryVersion]  WITH CHECK ADD  CONSTRAINT [FK_InfoQueryVersion_InfoQuery] FOREIGN KEY([QueryId])
REFERENCES [AES].[InfoQuery] ([QueryId])
GO
ALTER TABLE [AES].[InfoQueryVersion] CHECK CONSTRAINT [FK_InfoQueryVersion_InfoQuery]
GO



use master;    
drop database if exists StateStore
create database StateStore
GO
drop database if exists PubSubStore
create database PubSubStore
GO
drop database if exists ClusteringStore
create database ClusteringStore
GO
drop database if exists ReminderStore
create database ReminderStore
GO

DECLARE @current NVARCHAR(256);
DECLARE @snapshotSettings NVARCHAR(612);

SELECT @current = N'[' + (SELECT DB_NAME()) + N']';
SET @snapshotSettings = N'ALTER DATABASE ' + @current + N' SET READ_COMMITTED_SNAPSHOT ON; ALTER DATABASE ' + @current + N' SET ALLOW_SNAPSHOT_ISOLATION ON;';

EXECUTE sp_executesql @snapshotSettings;

IF OBJECT_ID(N'[OrleansQuery]', 'U') IS NULL
CREATE TABLE OrleansQuery
(
	QueryKey VARCHAR(64) NOT NULL,
	QueryText VARCHAR(8000) NOT NULL,

	CONSTRAINT OrleansQuery_Key PRIMARY KEY(QueryKey)
);
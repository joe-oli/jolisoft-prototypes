* **Server name:** `192.168.1.80\SQLEXPRESS`
* **Authentication:** `SQL Server Authentication`
* **Login:** `sa`
* **Password:** `Asma@Australia!69`


--
other login/pwd;
dev_user1 / dev_user1

--
Create a database (JolisoftDemoDB) and add a user 'dev_user1' to this database, and map the login dev_user1 to user dev_user1.

--
**UPDATE** 3/Oct/2026
There are now 2 SQL SERVERS on the LAN;
The Windows server will be removed; ANY NEW WORK should target the Linux SQL SERVER at 192.168.1.20;

**CURRENTLY** THERE ARE 2 SQL SERVER INSTANCES IN THE LAN;

SQL SERVER admin 'sa'
pwd = Asma@Australia!69


joe_oli@ubuntu24-sqlserver:~$ sqlcmd -S 192.168.1.80,1433 -U sa -P 'Asma@Australia!69' -C
1> SELECT @@SERVERNAME, @@VERSION;
2> GO
--------------------------------------------------------------------------------------  --------------------------------------------------
Win11-VM\SQLEXPRESS                                                                     Microsoft SQL Server 2025 (RTM) - 17.0.1000.7 (X64)
        Oct 21 2025 12:05:57
        Copyright (C) 2025 Microsoft Corporation
        Express Edition (64-bit) on Windows 10 Pro 10.0 <X64> (Build 26200: ) (Hypervisor)
(1 rows affected)
1> exit
joe_oli@ubuntu24-sqlserver:~$

---

```sh
sqlcmd -S 192.168.1.20,1433 -U sa -P 'Asma@Australia!69' -C

# DO THIS ONCE ONLY, to create a login at Server level;
CREATE LOGIN [dev_user1] WITH PASSWORD = 'dev_user1';
GO
```

1> CREATE LOGIN [dev_user1] WITH PASSWORD = 'dev_user1';
2> GO
Msg 33064, Level 16, State 2, Server ubuntu24-sqlserver, Line 1
Password validation failed. The password does not meet SQL Server password policy requirements because it is not complex enough. The password must be at least 8 characters long and contain characters from three of the following four sets: Uppercase letters, Lowercase letters, Base 10 digits, and Symbols.
1>

Try again
1> CREATE LOGIN [dev_user1] WITH PASSWORD = 'dev_user1', CHECK_POLICY = OFF;
2> GO
1>


If you are already logged in as 'sa', to change to a NEW PASSWORD simply enter:
```sh
ALTER LOGIN sa WITH PASSWORD = 'Asma@Australia!69';
GO
```


# How to apply migrations locally

Run these commands from the repository root:

```bash
dotnet ef migrations list
```

```bash
dotnet ef database update
```

Optional (generate SQL script):

```bash
dotnet ef migrations script
```

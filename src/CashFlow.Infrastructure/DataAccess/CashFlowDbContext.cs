using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAccess;

internal class CashFlowDbContext : DbContext
{
    public DbSet<Expense> Expenses { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = "Server=localhost;Database=cashflow_db;Uid=root;Pwd=@Password123;";
        
        // WSL
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            // Detecta se está rodando no WSL e tenta obter o IP do host
            var version = System.IO.File.Exists("/proc/version") ? System.IO.File.ReadAllText("/proc/version") : "";
            if (version.Contains("microsoft", StringComparison.CurrentCultureIgnoreCase))
            {
                var hostIp = GetWslHostIp();
                if (!string.IsNullOrEmpty(hostIp))
                {
                    connectionString = connectionString.Replace("localhost", hostIp);
                }
            }
        }

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 45));

        optionsBuilder.UseMySql(connectionString, serverVersion, options =>
        {
            options.EnableRetryOnFailure();
        });
    }

    private string GetWslHostIp()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sh",
                    Arguments = "-c \"ip route show | grep default | awk '{print $3}'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return result;
        }
        catch
        {
            return string.Empty;
        }
    }

}

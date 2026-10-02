$connStr = "Server=10.171.1.26\FGASQLPROD;Database=FGA;User Id=sa;Password=Estanoes1;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT @@SERVERNAME, DB_NAME()"
    $r = $cmd.ExecuteReader()
    if ($r.Read()) {
        Write-Output "CONNECTED: Server=$($r[0]), DB=$($r[1])"
    }
    $r.Close()
} catch {
    Write-Output "ERROR: $($_.Exception.Message)"
} finally {
    $conn.Close()
}

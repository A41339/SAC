$connStr = "Server=DESKTOP-6PESMK8;Database=FGA;Integrated Security=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()

    Write-Host "=== MENUS RAIZ (ParentId IS NULL) ==="
    $cmd.CommandText = "SELECT Id, MenuText, MenuURL, ParentId, SortOrder FROM dbo.Menu WHERE ParentId IS NULL ORDER BY SortOrder, Id"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "$($reader['Id']) | $($reader['MenuText']) | URL:$($reader['MenuURL']) | Sort:$($reader['SortOrder'])"
    }
    $reader.Close()

    Write-Host "`n=== HIJOS DE ADMINISTRACION (o Mantenimiento) ==="
    $cmd.CommandText = "SELECT Id, MenuText, MenuURL, ParentId, SortOrder FROM dbo.Menu WHERE ParentId = 9000 OR ParentId = 8000 OR ParentId = 7000 OR ParentId = 11000 ORDER BY ParentId, SortOrder, Id"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "Parent:$($reader['ParentId']) | Id:$($reader['Id']) | $($reader['MenuText']) | URL:$($reader['MenuURL'])"
    }
    $reader.Close()

    Write-Host "`n=== ROLES ACTIVOS Y PERMISOS ==="
    $cmd.CommandText = "SELECT mp.RoleId, r.RoleName, COUNT(*) as PermCount FROM dbo.MenuPermission mp LEFT JOIN dbo.Role r ON mp.RoleId = r.Id GROUP BY mp.RoleId, r.RoleName"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "RoleId: $($reader['RoleId']) ($($reader['RoleName'])) | Permisos: $($reader['PermCount'])"
    }
    $reader.Close()

    Write-Host "`n=== PERMISOS DE CADA ROL SOBRE MENUS RAIZ ==="
    $cmd.CommandText = "SELECT mp.RoleId, r.RoleName, m.Id, m.MenuText FROM dbo.MenuPermission mp JOIN dbo.Menu m ON mp.MenuId = m.Id LEFT JOIN dbo.Role r ON mp.RoleId = r.Id WHERE m.ParentId IS NULL ORDER BY mp.RoleId, m.SortOrder"
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "RoleId: $($reader['RoleId']) ($($reader['RoleName'])) -> Menu Raiz: $($reader['Id']) - $($reader['MenuText'])"
    }
    $reader.Close()

} finally {
    $conn.Close()
}

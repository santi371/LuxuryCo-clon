$controllers = Get-ChildItem -Path "LuxuryCo.Front/Controllers" -Filter "*.cs" -Recurse
foreach ($file in $controllers) {
    $content = Get-Content $file.FullName -Raw
    $content = $content -replace '"http://localhost:5137', '(Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5137") + "'
    $content = $content -replace '"https://localhost:7066', '(Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:7066") + "'
    Set-Content -Path $file.FullName -Value $content
}

$views = Get-ChildItem -Path "LuxuryCo.Front/Views" -Filter "*.cshtml" -Recurse
foreach ($file in $views) {
    $content = Get-Content $file.FullName -Raw
    $content = $content -replace "'http://localhost:5137", "'@(Environment.GetEnvironmentVariable(""API_BASE_URL"") ?? ""http://localhost:5137"")"
    $content = $content -replace "'https://localhost:7066", "'@(Environment.GetEnvironmentVariable(""API_BASE_URL"") ?? ""https://localhost:7066"")"
    Set-Content -Path $file.FullName -Value $content
}

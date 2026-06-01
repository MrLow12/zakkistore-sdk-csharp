#!/bin/bash
# Script helper untuk mempublikasikan C# NuGet package Zakki Store SDK ke NuGet.org

if [ -z "$NUGET_API_KEY" ]; then
  read -sp "Masukkan NuGet API Key Anda: " NUGET_API_KEY
  echo ""
fi

if [ -z "$NUGET_API_KEY" ]; then
  echo "❌ API Key tidak boleh kosong."
  exit 1
fi

echo "🛠️ Membangun NuGet package..."
# Diperlukan dotnet SDK untuk menjalankan perintah ini
dotnet pack --configuration Release

if [ $? -eq 0 ]; then
  echo "📤 Mengunggah package ke NuGet.org..."
  dotnet nuget push bin/Release/*.nupkg --api-key "$NUGET_API_KEY" --source https://api.nuget.org/v3/index.json
else
  echo "❌ Gagal memaketkan NuGet package. Pastikan .NET SDK terpasang."
  exit 1
fi

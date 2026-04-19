# 1. שלב הבנייה
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# העתקת קובץ הפרויקט מתוך התיקייה הפנימית
# במקום השורות הקודמות, נסי את זה בשלב ה-Build:
COPY ["**/*.csproj", "./"]
RUN dotnet restore

# העתקת כל שאר הקבצים
COPY . .
WORKDIR "/src/TodoApi"
RUN dotnet publish "TodoApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. שלב ההרצה
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# הגדרת פורט עבור Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# הרצת ה-DLL שנוצר
ENTRYPOINT ["dotnet", "TodoApi.dll"]
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. מעתיקים את כל הקבצים של הפרויקט לסביבת הבנייה
COPY . .

# 2. נכנסים לתיקייה שבה נמצא קובץ הפרויקט (לפי מה ששלחת קודם)
WORKDIR "/src/TodoApi"

# 3. מריצים שחזור ובנייה מתוך התיקייה עצמה
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 4. שלב ההרצה
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TodoApi.dll"]
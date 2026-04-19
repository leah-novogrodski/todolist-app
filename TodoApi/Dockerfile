# 1. שלב הבנייה (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# העתקת קובץ הפרויקט ושחזור חבילות (Restore)
# החליפי את YourProjectName בשם הפרויקט שלך
COPY ["YourProjectName.csproj", "./"]
RUN dotnet restore "YourProjectName.csproj"

# העתקת כל שאר הקבצים ובניית הפרויקט
COPY . .
RUN dotnet publish "YourProjectName.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. שלב ההרצה (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# הגדרת פורט (Render מצפה לפורט 8080 כברירת מחדל ב-Docker)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# פקודת ההרצה של השרת
ENTRYPOINT ["dotnet", "YourProjectName.dll"]
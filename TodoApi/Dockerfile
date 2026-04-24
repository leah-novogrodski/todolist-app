# שלב הבנייה
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# העתקת קובץ הפרויקט (הוא נמצא באותה תיקייה של ה-Dockerfile כרגע)
COPY ["TodoApi.csproj", "./"]
RUN dotnet restore "TodoApi.csproj"

# העתקת כל שאר הקבצים בתיקייה
COPY . .
RUN dotnet publish "TodoApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# שלב ההרצה
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# הגדרת פורט

EXPOSE 8080

ENTRYPOINT ["dotnet", "TodoApi.dll"]
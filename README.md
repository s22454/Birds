
## Uruchomienie projektu

### Wersja .NET 

Projekt wykorzystuje środowisko .NET w wersji 7, więc aby zapewnić działanie aplikacji należy je zainstalować. Można się dowiedzieć jaką wersje .NET mamy zainstalowaną wykonując w konsoli następujące polecenie.

```terminal
dotnet --info
```

Jeśli wymagana wersja .NET nie jest zainstalowana na naszym komputerze należy ją pobrać, instrukcje jak dokładnie to zrobić można znaleźć na stronie https://learn.microsoft.com/pl-pl/dotnet/core/install/windows?tabs=net80. 

### Kontener Dockera

Aby uruchomić projekt należy najpierw utworzyć i uruchomić kontener dockera z instancją bazy danych MS SQL Server. Jeśli jeszcze nie posiadamy obrazu bazy na komputerze to pierwszym krokiem jest pobranie go za pomocą następującej komendy (wpisanej w terminalu). 

```terminal
docker pull mcr.microsoft.com/mssql/server
```

Kolejnym krokiem jest właściwe stworzenie i uruchomienie kontenera.

```terminal
docker run --name "BirdsMsSQLServer" -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Bardzomocnehaslo1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### Wytrenowany model 

Aby zapewnić poprawne działanie aplikacji plik z wytrenowanym modelem *bigtest1.zip* należy umieścić w pod-projekcie *Website* w folderze *ML*. 

### Instalacja zależności 

IDE powinno automatycznie rozpoznać i zainstalować wszystkie potrzebne pakiety za pomocą NuGet managera. Jedyny wyjątek stanowi pakiet EntityFramework tools który, należy zainstalować na poziomie globalnym. 

```terminal
dotnet tool install --global dotnet-ef
```
### Inicjacja bazy danych 

Aby zainicjować bazę danych najpierw należy pozbyć się (usunąć) niepotrzebnych migracji z folderu *Migrations* z pod-projektu *Birds.EntityFramework*.

Kiedy folder jest już pusty można wykonać update bazy. Można to zrobić z poziomu IDE *prawy przycisk myszy na pod-projekt Website → Entity Framework Core → Add Migration → name: Init; Migrations project: Birds.EntityFramework; Startup project: Website; → Ok* i następnie *prawy przycisk myszy na pod-projekt Website → Entity Framework Core → Update Database*

### Odpalenie projektu 

Na tym etapie projekt jest gotowy do uruchomienia, przy pierwszym starcie strona może się nie załadować ze względu na proces ustanawiania połączenia z bazą danych przez aplikacje. Należy wtedy odczekać 2/3s i odświeżyć stronę. 
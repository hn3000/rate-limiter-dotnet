# Strong Named Assembly:

create a key using sn util in RateLimiter folder: 

sn -k key.snk

add key to github secrets (base64 encoded)

base64 < RateLimiter/key.snk > RateLimiter/key.snk.base64

secret name: NUGET_SIGNING_KEY




# Try to pack locally: 

dotnet pack RateLimiter --version-suffix "test" --configuration Release --output ./artifacts


# Nuget release

make sure to add your nuget api key to github secrets

secret name: NUGET_API_KEY

## pre-release

develop will autom. create pre-releases using current version number and appending rc.<datetime>

## full release

increase version numer in RateLimiter.csproj > VersionPrefix
merge to master will autom. create release with version number in csproj
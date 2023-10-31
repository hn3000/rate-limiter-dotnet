# Strong Named Assembly:

create a key using sn util in RateLimiter folder: 


either
```
sn -k key.snk
```

or
```
openssl genpkey -algorithm RSA -out privatekey.pem
openssl rsa -pubout -in privatekey.pem -out publickey.pem
openssl rsa -in privatekey.pem -outform 'MS PUBLICKEYBLOB' -out key.snk
```


add key to github secrets (base64 encoded)

base64 < RateLimiter/key.snk > RateLimiter/key.snk.base64

secret name: NUGET_SIGNING_KEY

gh secret set NUGET_SIGNING_KEY --repo hn3000/rate-limiter-dotnet < RateLimiter/key.snk.base64


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
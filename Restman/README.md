# Restman

A tool to help you to do commonly-used REST commands from the command line.

Requests are defined in a "spec file" which defines the types of requests that Restman knows about.

Requests can take arguments which are used for token replacement against the parameterized REST requests.

## Spec file format
By default, Restman reads a file called `.restman.json` from either your user directory (`~/.restman.json`) or the current directory.

    {
        "variableSets": {
            "default": {
                "host": "https://prod.myservice.com/",
                "locale": "en-US",
                "version": "v1"
            },
            "test": {
                "host": "https://test.myservice.com/",
            },
            "local": {
                "host": "http://localhost:5000/",
            },
            "ireland": {
                "locale": "en-IE"
            }
        },
        "requests": {
            "getaccount": {
                "url": "{{host}}/api/{{version}}/account/{{accountid}}?locale={{locale}}",
                "ordinalArgs": [ "accountid", "locale" ]
            }
        }
    }

### Variable sets
The spec contains "variable sets" which are sets of name value pairs that will be used for token substitution. Variable sets can be "stacked", so you can request multiple variable sets, which will all be merged together. 

For example, if you run the command `restman getaccount -v test,ireland 19293`, then it will load the variables from "default", "test", and "ireland." The resulting variables will be: `host=https://test.myservice.com`, `version=v1`, and `locale=en-IE`.

### Requests

Requests define each of the requests that Restman knows about. Each request defines the following properties:

* url: The URL for the REST request. Any tokens in the form of `{{variable}}` will be substituted from the set of variables.
* method: (optional) - the HTTP method to use (the default is GET)
* headers: (optional) - an object that represents the set of HTTP request headers to pass along. Token substitution is also applied to headers.
* ordinalArgs: (optional) - if unnamed variables are passed on the command line, they will be assigned to these variables. If you don't pass all ordinal arguments, then the default values will be used for those variables.
* bifoqlQuery: (optional) - if the result is a JSON object, then the result will be passed through a Bifoql query, which if you've never heard of it, is an awesome language for taking JSON objects and mapping them into something else.
  
## Usage

`restman requestName [-f specfile] [-v variableset1,variableset2...] [-b bifoqlQuery] [ordinalArgs...] [namedArg1=value1...]`

The command takes a request name as a first parameter. It can take arguments either by order, or named.

You can also pass "variable sets", which are sets of variables that are defined for different scenarios (for example, you can have a "test" environment which allows you to run the command against your test environment.)

If you pass a bifoql query, then it will override the bifoql query defined for the request (if defined.) If you'd like to get the raw data for a request, pass a bifoql query of "@"

### Examples:

#### Simple case
`restman getaccount 19293 fr-FR`

#### Passing named arguments
`restman getaccount accountid=19293 locale=fr-FR`

#### Using variable sets
`restman getaccount -v test 19293 fr-FR`

`restman getaccount -v test,ireland 19293`

#### Passing a bifoql query
`restman getaccount -b "{ name, address }"`

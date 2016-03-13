# TheDailyWTF API

### Version
0.1

### Notes
* Refer [ArticleModel](https://github.com/tdwtf/WtfWebApp/blob/master/TheDailyWtf/Models/ArticleModel.cs) class for description of terms used below.
* The API returns data in JSON format with content type *application/json*.
* Wherever applicable the count parameter has been limited to 100. The command shall return the specified count or the maximum available number of article (whichever is lesser).
* The commands return a Status field specifying exceptions if any.
* The API supports two types of commands : **Listing Commands** and **Display Commands**
* **Listing Commands**
  * Return a list of articles in a json array.
  * The BodyHtml, BodyAndAdHtml and FooterAdHtml attributes of each ArticleModel object are replaced with an empty string.
  * This was done to reduce the amount of data returned during calls to get data to populate lists.
* **Display Commands**
  * These commands get all the information about a specific article.
  * They also have parameters to allow return of only BodyHtml and FooterAdHtml attributes of ArticleModel object, to complete the information obtained from a listing command, for any particular valid article.
* Base url for any api command shall be **http://thedailywtf.com/api** to which the below mentioned commands must be concatenated.
  * example api call: http://thedailywtf.com/api/articles/slug/the-apple-genius

### Bugs/Requests
Please create a new issue [here](https://github.com/tdwtf/WtfWebApp/issues)

## Commands

### Display Commands

#### 1. /articles/id/{id}

* Description
  * Returns the article with the specified id 
* Parameters
  * **id** : integer : Unique identifier of the article
* Example
  * http://thedailywtf.com/api/articles/id/8301
* Exceptions
  *  { "Status": "Invalid Id" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 2. /articles/id/{id}/{onlyBodyAndAdHtml}

* Description
  * Returns only the BodyHtml and FooterAdHtml attributes of the article with the specified id
* Parameters
  * **id** : integer : Unique identifier of the article
  * **onlyBodyAndAdHtml** : bool : Default value is false.
* Example
  * http://thedailywtf.com/api/articles/id/8301/true
* Exceptions
  *  { "Status": "Invalid Id" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 3. /articles/slug/{articleSlug}

* Description
  * Returns the article with the specified article slug
* Parameters
  * **articleSlug** : string : Article Slug
* Example
  * http://thedailywtf.com/api/articles/slug/your-recommended-virus
* Exceptions
  *  { "Status": "Invalid Article Slug" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 4. /articles/slug/{articleSlug}/{onlyBodyAndAdHtml}

* Description
  * Returns only the BodyHtml and FooterAdHtml attributes of the article with the specified article slug
* Parameters
  * **articleSlug** : string : Article Slug
  * **onlyBodyAndAdHtml** : bool : Default value is false.
* Example
  * http://thedailywtf.com/api/articles/slug/your-recommended-virus/true
* Exceptions
  *  { "Status": "Invalid Article Slug" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 5. /articles/random

* Description
  * Returns a random article
* No Parameters
* Example
  * http://thedailywtf.com/api/articles/random
* Exceptions
  *  { "Status": "Service Unavailable" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

### Listing Commands

#### 6. /articles/recent

* Description
  * Returns an array of **8 recent** articles
* No Parameters
* Example
  * http://thedailywtf.com/api/articles/recent
* Exceptions
  *  { "Status": "Service Unavailable" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 7. /articles/recent/{count}

* Description
  * Returns an array of {count} number of recent articles
* Parameters
  * **count** : int
* Example
  * http://thedailywtf.com/api/articles/recent/15
* Exceptions
  *  { "Status": "Service Unavailable" }
  *  { "Status": "Count cannot be greater than 100" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 8. /articles/{year}/{month}

* Description
  * Returns an array of articles published in the specified month of the specified year
* Parameters
  * **year** : int : ideally > 1999
  * **month** : int : 1-12
* Example
  * http://thedailywtf.com/api/articles/2015/12
* Exceptions
  *  { "Status": "No articles found for the current date range" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 9. /series/{slug}

* Description
  * Returns an array of **8 recent** articles in the series specified in {slug}
* Parameters
  * **slug** : string : eg. code-sod, errord, feature-articles
* Example
  * http://thedailywtf.com/api/series/code-sod
* Exceptions
  *  { "Status": "Invalid Series" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 10. /series/{slug}/{count}

* Description
  * Returns an array of {count} number of recent articles in the series specified in {slug}
* Parameters
  * **slug** : string : eg. code-sod, errord, feature-articles
  * **count** : int
* Example
  * http://thedailywtf.com/api/series/code-sod/42
* Exceptions
  *  { "Status": "Invalid Series" }
  *  { "Status": "Count cannot be greater than 100" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 11. /series/{slug}/{year}/{month}

* Description
  * Returns an array of articles published in the specified month of the specified year in the specified series
* Parameters
  * **slug** : string : eg. code-sod, errord, feature-articles
  * **year** : int : ideally > 1999
  * **month** : int : 1-12
* Example
  * http://thedailywtf.com/api/series/code-sod/2015/12
* Exceptions
  *  { "Status": "No articles found for the current date range or Invalid Series" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 12. /author/{slug}

* Description
  * Returns an array of **8 recent** articles by the author specified in {slug}
* Parameters
  * **slug** : string : eg. alex-papadimoulis, remy-porter
* Example
  * http://thedailywtf.com/api/author/alex-papadimoulis
* Exceptions
  *  { "Status": "Invalid Author" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 13. /author/{slug}/{count}

* Description
  * Returns an array of {count} number of recent articles by the author specified in {slug}
* Parameters
  * **slug** : string : eg. alex-papadimoulis, remy-porter
  * **count** : int
* Example
  * http://thedailywtf.com/api/author/alex-papadimoulis/11
* Exceptions
  *  { "Status": "Invalid Author" }
  *  { "Status": "Count cannot be greater than 100" }
  *  { "Status": "JSON Serialization Error : <Error Message>" }

#### 14. /series/

* Description
  * Returns an array of all available series
* Example
  * http://thedailywtf.com/api/series/
* Exceptions
  *  { "Status": "JSON Serialization Error : <Error Message>" }

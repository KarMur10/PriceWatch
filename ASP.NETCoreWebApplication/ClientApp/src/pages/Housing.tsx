﻿import React, {useEffect, useState} from "react";
import {Layout} from "../components/Layout";
import {getHousingObjects, getHousingObjectsMySQL} from "../utils/RoutePaths";
import {Button, Card, Checkbox, FormControlLabel, Switch, TextField} from "@material-ui/core";
import LinearProgress from "../components/common/LinearProgress";
import ColAuto from "../components/common/ColAuto";
import Row from "../components/common/Row";
import Next from "../components/common/Next";
import ColoredLinearProgress from "../components/common/LinearProgress";
import SortComponent from "../components/common/SortComponent";

//import '../../../node_modules/bootstrap/dist/css/bootstrap.min.css';

interface IHousingObject {
    title: string,
    url: string,
    price: number,
    location: string,
    currency: string,
    area: number,
    floorsMax: number,
    floorsThis: number,
    description: string,
    imgUrl: string
}

function capitalize(input: string): string {
    const [first, ...rest] = input.split('');
    if(input == '') return '';
    if(input.length == 1) return first.toUpperCase();
    return first.toUpperCase() + rest.map((r: string) => r.toLowerCase()).join('');
}

export default function (props: any): JSX.Element {
    const [error, setError] = useState<Error | PositionError | null>(null);
    const [housingObjects, setHousingObjects] = useState<IHousingObject[]>([]);
    //search keys
    const [searchKey, setSearchKey] = useState<string>('');
    const [priceMin, setPriceMin] = useState<number>(0);
    const [priceMax, setPriceMax] = useState<number>(100000);
    const [roomsMin, setRoomsMin] = useState<number>(1);
    const [roomsMax, setRoomsMax] = useState<number>(5);
    const [floors, setFloors] = useState<number>(2);
    const [fetching, setFetching] = useState<boolean>(false);
    const [searchInDescription, setSeatchInDescription] = useState<boolean>(false);
    const params: RequestInit = { headers: {'Content-Type': 'application/json'} };
    
    function removeDuplicatesOnURLKey(data1: IHousingObject[], data2: IHousingObject[]) {
        //URL will always be unique so we remove duplicates
        let primaryData: IHousingObject[] = [...data1];
        let urls: string[] = primaryData.map((obj: IHousingObject) => obj.url);
        data2.forEach((obj: IHousingObject) => {
            if(!urls.includes(obj.url)){
                primaryData.push(obj);
            }
        });
        return primaryData;
    }
    
    async function fetchLocation() {
        setFetching(true);
        return await new Promise((resolve, reject) => {
            navigator.geolocation.getCurrentPosition((position) => {
                setFetching(false);
                resolve(position.coords);
            }, (err) => {
                setFetching(false);
                setError(err);
            });
        });
        //
    }
    
    async function navigateTo(src: string){
        window.open(src, '_blank');
    }

    async function fetchHousingObjectList(t?: any) {
        // const token = await authService.getAccessToken();
        // {
        //     headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        // }
        setFetching(true);
        
        const response = await fetch(getHousingObjectsMySQL + (
            params ? '?' +Object.keys(t).reduce((prevStr: string, param: string) => {
                return prevStr + param + '=' + t[param] + '&';
            }, '').slice(0, -1) : ''
        ));
        const data = await response.json();
        setHousingObjects(data);
        setFetching(false);
    }

    async function fetchHousingObjectListScrapper(t?: any) {
        // const token = await authService.getAccessToken();
        // {
        //     headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        // }
        setFetching(true);

        const response = await fetch(getHousingObjects + (
            params ? '?' +Object.keys(t).reduce((prevStr: string, param: string) => {
                return prevStr + param + '=' + t[param] + '&';
            }, '').slice(0, -1) : ''
        ));
        const data = await response.json();
        let additionalData: IHousingObject[] = removeDuplicatesOnURLKey(housingObjects, data);
        
        setHousingObjects(additionalData);
        setFetching(false);
    }
    
    useEffect(() => {
        fetchHousingObjectList({
            searchKey,
            priceMin,
            priceMax,
            roomsMin,
            roomsMax,
            floors,
            searchInDescription
        });
    }, [searchKey, priceMin, priceMax, roomsMin, roomsMax, floors, searchInDescription]);

    // return <div>{JSON.stringify(props)}</div>;
    return <Layout>
        <Card>
            <h4>Search & Filter:</h4>
        <div className={'form-group row pl-4 pr-4'}>
            <div className={"col-lg-3 pw-search-form-group p-3"}>
                    <TextField id="outlined-basic" label="Search key" variant="standard" value={searchKey} onChange={(e) => setSearchKey(capitalize(e.target.value))}/>
                    <FormControlLabel control={<Switch checked={searchInDescription} onChange={(e) => setSeatchInDescription(!searchInDescription)}/>} label="Search in description" color={"secondary"}/>
            </div>
            <div className={"col-lg-3 pw-search-form-group p-3"}>
                    <TextField id="outlined-basic" label="Min price" variant="standard" value={priceMin} onChange={(e) => setPriceMin(parseInt(e.target.value) || 0)}/><br/>
                    <TextField id="outlined-basic" label="Max price" variant="standard" value={priceMax} onChange={(e) => setPriceMax(parseInt(e.target.value) || 0)}/>
            </div>
            <div className={"col-lg-3 pw-search-form-group p-3"}>
                <TextField id="outlined-basic" label="Floors min" variant="standard" />
                <TextField id="outlined-basic" label="Floors max" variant="standard" />
            </div>
            <div className={"col-lg-3 pw-search-form-group p-3"}>
                <TextField id="outlined-basic" label="Rooms min" variant="standard" value={roomsMin} onChange={(e) => setRoomsMin(parseInt(e.target.value))}/><br/>
                <TextField id="outlined-basic" label="Rooms max" variant="standard" value={roomsMax} onChange={(e) => setRoomsMax(parseInt(e.target.value))}/>
            </div>
            <div className={"w-100"}>
                
            </div>
            {/*<div className={"col-lg-4 pw-search-form-group"}>*/}
            {/*</div>*/}
            <div className={"w-100"}>
            </div>
            <div className={"row pl-4 pr-4 w-100"}>
                <div className={"col-auto mr-auto"}>
                    
                <FormControlLabel
                    value="aruodas"
                    color={"secondary"}
                    control={<Checkbox />}
                    label="Aruodas.lt"
                    labelPlacement="end"
                />
                <FormControlLabel
                    value="alio"
                    color={"secondary"}
                    control={<Checkbox />}
                    label="Alio.lt"
                    labelPlacement="end"
                />
                </div>
                
                <div className={"col-auto"}>
                    <Button onClick={() => fetchHousingObjectListScrapper(
                        {
                            searchKey,
                            priceMin,
                            priceMax,
                            roomsMin,
                            roomsMax,
                            floors
                        }
                    )} variant={"outlined"}>Force scan</Button>
                </div>
            </div>
        </div>
    </Card>
        <Card>
            <h4>Sort keys:</h4>
            <div className={'row pl-4 pr-4'}>
                <SortComponent array={housingObjects} stateCallback={setHousingObjects} predicate={
                    (h1: IHousingObject, h2: IHousingObject) => JSON.parse(JSON.stringify(housingObjects))
                        .sort((h1: IHousingObject, h2: IHousingObject) => h1.price - h2.price)} label={"Price"} />
                <SortComponent array={housingObjects} stateCallback={setHousingObjects} predicate={async () => {
                    const r = await fetchLocation();
                    return housingObjects;
                }} label={"Location"} />
            </div>
        </Card>
        {fetching && <ColoredLinearProgress />}
        {error ? <div className="alert alert-danger" role="alert">
            <h4 className="alert-heading">Error!</h4>
            {error.message}
        </div> : (housingObjects.length) && (<div>
            {housingObjects.map((house: IHousingObject) => (
                <Card>
                    <Row>
                        <ColAuto pushOthersToRight>
                            <Row fullWidth>
                                <ColAuto pushOthersToRight>
                                    <h5>{house.title}</h5>
                                </ColAuto>
                                <ColAuto pushToRight={true}>
                                    <Button onClick={() => navigateTo(house.url)} endIcon={<Next />} variant={"outlined"}>
                                        Explore
                                    </Button>
                                </ColAuto>
                            </Row>
                            <h5><i className="fas fa-map-marked-alt"></i>{'  '}{house.location}</h5>
                             <h5><i className="fas fa-coins"></i>{'  '}{house.price}{' '}{house.currency}</h5>
                            <h5><i className="fas fa-building"></i>{'  '}{house.floorsThis}{'/'}{house.floorsMax}</h5>
                        </ColAuto>
                        <ColAuto >
                            <img className="float-right" src={house.imgUrl}/>
                        </ColAuto>
                    </Row>
                </Card>
            ))}
        </div>)}
    </Layout>
}
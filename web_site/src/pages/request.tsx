import { AuthStatus, GetRequestFriendsDto } from "@/Hook/type";
import Loader1 from "../app/components/Loader1";
import { UseAuth } from "@/Hook/UseAuth";
import 'tailwindcss/tailwind.css';
import { useRouter } from 'next/navigation'
import { useEffect, useState } from 'react';
import Head from 'next/head';
import Image from 'next/image';

export default function Request() {



    const { requestFriendsDto, status, userDto, GetRequestFriends, UserGetConnected, AcceptRequestFriends } = UseAuth();
    const router = useRouter();



    const [sentRequests, setSentRequests] = useState<GetRequestFriendsDto[]>([]);
    const [receivedRequests, setReceivedRequests] = useState<GetRequestFriendsDto[]>([]);
    const [friends, setFriends] = useState<GetRequestFriendsDto[]>([]);




    useEffect(() => {
        GetRequestFriends();
        UserGetConnected();
        let timer: NodeJS.Timeout | undefined;
        if (status === AuthStatus.Unauthenticated) {
            timer = setTimeout(() => {
                router.push('/login');
            }, 3000);
        }
        return () => clearTimeout(timer);
    }, [status, UserGetConnected, GetRequestFriends ,router]);




    useEffect(() => {
        if (requestFriendsDto) {
            const sent: GetRequestFriendsDto[] = [];
            const received: GetRequestFriendsDto[] = [];
            const friendsList: GetRequestFriendsDto[] = [];
            requestFriendsDto.forEach((item: GetRequestFriendsDto) => {
                if (item.idUserIssuer === userDto?.id_User && item.status === 0) {
                    sent.push(item);
                } else if (item.id_UserReceiver === userDto?.id_User && item.status === 0) {
                    received.push(item);
                } else if (item.status === 1) {
                    friendsList.push(item);
                }
            });
            setSentRequests(sent);
            setReceivedRequests(received);
            setFriends(friendsList);
        }
    }, [requestFriendsDto, userDto]);




    if (status === AuthStatus.Authenticated) {
        return (
            <div className="bg-pink-200 flex flex-col items-center justify-center h-screen sm:p-6">
                <Head>
                    <title>AmourConnect</title>
                    <link rel="icon" href="/assets/images/amour_connect_logo.jpg" />
                </Head>
                <div className="w-full max-w-4xl mx-auto shadow-lg rounded-lg overflow-hidden">
                    <div className="flex flex-col md:flex-row">
                        <div className="w-full md:w-1/3 p-4">
                            <h2 className="text-lg font-medium text-gray-900 mb-2 bg-white">Demandes de match envoyees en attente</h2>
                            <table className="w-full text-left divide-y divide-gray-200">
                                <tbody className="divide-y divide-gray-200">
                                    {sentRequests.map((item, index) => (
                                        <tr key={index}>
                                            <td className="px-6 py-4 whitespace-nowrap ">
                                                <div className="flex items-center">
                                                    <div className="ml-4">
                                                        <a href={`/profil-details/${item.id_UserReceiver}`}>
                                                            <div className="text-sm font-medium text-gray-900">{item.userReceiverPseudo}</div>
                                                        </a>
                                                        <div className="text-sm text-gray-500">{new Date(item.date_of_request).toLocaleString()}</div>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                        <div className="w-full md:w-1/3 p-4">
                            <h2 className="text-lg font-medium text-gray-900 mb-2 bg-white">Demandes de match recues</h2>
                            <table className="w-full text-left divide-y divide-gray-200">
                                <tbody className="divide-y divide-gray-200">
                                    {receivedRequests.map((item, index) => (
                                        <tr key={index}>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="flex items-center">
                                                    <div className="ml-4">
                                                        <a href={`/profil-details/${item.idUserIssuer}`}>
                                                            <div className="text-sm font-medium text-gray-900">{item.userIssuerPseudo}</div>
                                                        </a>
                                                        <div className="text-sm text-gray-500">{new Date(item.date_of_request).toLocaleString()}</div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-right">
                                                <button
                                                    className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                                    onClick={() => AcceptRequestFriends(item.idUserIssuer)}
                                                >
                                                    Accepter
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                        <div className="w-full md:w-1/3 p-4">
                            <h2 className="text-lg font-medium text-gray-900 mb-2 bg-white">Liste de match valides</h2>
                            <table className="w-full text-left divide-y divide-gray-200">
                                <tbody className="divide-y divide-gray-200">
                                    {friends.map((item, index) => (
                                        <tr key={index}>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="flex items-center">
                                                    <div className="ml-4">
                                                        <a href={`/profil-details/${item.idUserIssuer === userDto?.id_User ? item.id_UserReceiver : item.idUserIssuer}`}>
                                                            <div className="text-sm font-medium text-gray-900">{item.userIssuerPseudo === userDto?.pseudo ? item.userReceiverPseudo : item.userIssuerPseudo}</div>
                                                        </a>
                                                        <a href={`/tchat/${item.idUserIssuer === userDto?.id_User ? item.id_UserReceiver : item.idUserIssuer}`}>
                                                            <Image
                                                                src="/assets/images/tchat_icon.svg"
                                                                alt="Tchater avec"
                                                                width={20}
                                                                height={20}
                                                            />
                                                        </a>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <Loader1 />
    );
}
